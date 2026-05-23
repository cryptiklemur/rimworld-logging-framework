import { describe, expect, it, vi } from 'vitest';
import { createGist } from '../src/gist';

describe('createGist', () => {
  it('POSTs to api.github.com/gists with auth headers', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ html_url: 'https://gist.github.com/u/abc', id: 'abc' }), { status: 201 }),
    );
    const r = await createGist('tok', { description: 'd', public: false, files: { 'a.txt': { content: 'hi' } } }, fetchMock);
    expect(fetchMock).toHaveBeenCalledOnce();
    const [url, init] = fetchMock.mock.calls[0]!;
    expect(url).toBe('https://api.github.com/gists');
    expect((init as RequestInit).method).toBe('POST');
    const headers = (init as RequestInit).headers as Record<string, string>;
    expect(headers.Authorization).toBe('Bearer tok');
    expect(headers['User-Agent']).toBe('rimlogging-bundle-worker');
    expect(r.ok).toBe(true);
    if (r.ok) expect(r.url).toBe('https://gist.github.com/u/abc');
  });

  it('serializes the request body as JSON', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ html_url: 'u', id: 'i' }), { status: 201 }),
    );
    await createGist('tok', { description: 'd', public: false, files: { 'a': { content: 'b' } } }, fetchMock);
    const init = fetchMock.mock.calls[0]![1] as RequestInit;
    const parsed = JSON.parse(init.body as string);
    expect(parsed).toEqual({ description: 'd', public: false, files: { a: { content: 'b' } } });
  });

  it('returns failure on non-2xx', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response('forbidden', { status: 403 }));
    const r = await createGist('tok', { description: 'd', public: false, files: {} }, fetchMock);
    expect(r.ok).toBe(false);
    if (!r.ok) {
      expect(r.status).toBe(403);
      expect(r.body).toBe('forbidden');
    }
  });

  it('returns failure when response has no html_url', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify({ id: 'i' }), { status: 201 }));
    const r = await createGist('tok', { description: 'd', public: false, files: {} }, fetchMock);
    expect(r.ok).toBe(false);
  });

  it('returns failure on malformed JSON', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response('not json', { status: 201 }));
    const r = await createGist('tok', { description: 'd', public: false, files: {} }, fetchMock);
    expect(r.ok).toBe(false);
  });
});
