import { env } from 'cloudflare:test';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { handle } from '../src/index';

function post(body: unknown, opts: { ip?: string; contentType?: string; contentLength?: string } = {}): Request {
  const headers: Record<string, string> = {
    'content-type': opts.contentType ?? 'application/json',
    'cf-connecting-ip': opts.ip ?? '1.1.1.1',
  };
  if (opts.contentLength) headers['content-length'] = opts.contentLength;
  return new Request('https://example.com/v1/bundle', {
    method: 'POST',
    headers,
    body: typeof body === 'string' ? body : JSON.stringify(body),
  });
}

function gistOk() {
  return vi.fn().mockImplementation(async () =>
    new Response(JSON.stringify({ html_url: 'https://gist.github.com/u/abc', id: 'abc' }), { status: 201 }),
  );
}

const validBundle = () => ({
  rimWorldVersion: '1.5',
  frameworkVersion: '1.0',
  mods: [],
  entries: [{ ts: 't', level: 'Info', channel: 'c', source: '', msg: 'm' }],
});

describe('handle', () => {
  beforeEach(async () => {
    const list = await env.RATELIMIT.list();
    for (const k of list.keys) await env.RATELIMIT.delete(k.name);
  });

  it('returns 200 with gist URL on success', async () => {
    const resp = await handle(post(validBundle()), env, gistOk());
    expect(resp.status).toBe(200);
    const j = await resp.json() as { url: string; id: string };
    expect(j.url).toBe('https://gist.github.com/u/abc');
    expect(j.id).toBe('abc');
  });

  it('returns 405 for non-POST', async () => {
    const req = new Request('https://example.com/v1/bundle', { method: 'GET' });
    const resp = await handle(req, env, gistOk());
    expect(resp.status).toBe(405);
  });

  it('returns 404 for unknown path', async () => {
    const req = new Request('https://example.com/other', { method: 'POST', headers: { 'content-type': 'application/json' } });
    const resp = await handle(req, env, gistOk());
    expect(resp.status).toBe(404);
  });

  it('returns 415 for wrong content-type', async () => {
    const resp = await handle(post(validBundle(), { contentType: 'text/plain' }), env, gistOk());
    expect(resp.status).toBe(415);
  });

  it('returns 413 when content-length exceeds cap', async () => {
    const resp = await handle(post(validBundle(), { contentLength: String(6 * 1024 * 1024) }), env, gistOk());
    expect(resp.status).toBe(413);
  });

  it('returns 400 on malformed JSON', async () => {
    const resp = await handle(post('{not json', { ip: '2.2.2.2' }), env, gistOk());
    expect(resp.status).toBe(400);
  });

  it('returns 400 on schema failure', async () => {
    const resp = await handle(post({ rimWorldVersion: 1 }, { ip: '3.3.3.3' }), env, gistOk());
    expect(resp.status).toBe(400);
    const j = await resp.json() as { error: string; details: string[] };
    expect(j.error).toBe('validation failed');
    expect(j.details.length).toBeGreaterThan(0);
  });

  it('returns 429 after the rate limit', async () => {
    const f = gistOk();
    for (let i = 0; i < 10; i++) {
      const r = await handle(post(validBundle(), { ip: '4.4.4.4' }), env, f);
      expect(r.status).toBe(200);
    }
    const blocked = await handle(post(validBundle(), { ip: '4.4.4.4' }), env, f);
    expect(blocked.status).toBe(429);
    expect(blocked.headers.get('retry-after')).toBeTruthy();
  });

  it('returns 502 when github rejects', async () => {
    const f = vi.fn().mockResolvedValue(new Response('boom', { status: 500 }));
    const resp = await handle(post(validBundle(), { ip: '5.5.5.5' }), env, f);
    expect(resp.status).toBe(502);
  });

  it('posts three named files to github', async () => {
    const f = gistOk();
    await handle(post(validBundle(), { ip: '6.6.6.6' }), env, f);
    const init = f.mock.calls[0]![1] as RequestInit;
    const parsed = JSON.parse(init.body as string) as { files: Record<string, unknown> };
    expect(Object.keys(parsed.files).sort()).toEqual(['bundle.json', 'logs.txt', 'summary.md']);
  });
});
