import { createGist, type FetchLike } from './gist';
import { checkRateLimit } from './ratelimit';
import { renderLogs } from './render/logs';
import { renderSummary } from './render/summary';
import { MAX_BODY_BYTES, validateBundle } from './validate';

export type Env = {
  RATELIMIT: KVNamespace;
  GITHUB_TOKEN: string;
};

export default {
  async fetch(req: Request, env: Env): Promise<Response> {
    return handle(req, env);
  },
};

export async function handle(req: Request, env: Env, fetchImpl: FetchLike = fetch): Promise<Response> {
  if (req.method !== 'POST') return json({ error: 'method not allowed' }, 405);

  const url = new URL(req.url);
  if (url.pathname !== '/v1/bundle') return json({ error: 'not found' }, 404);

  const ct = req.headers.get('content-type') ?? '';
  if (!ct.includes('application/json')) return json({ error: 'expected application/json' }, 415);

  const cl = req.headers.get('content-length');
  if (cl && Number.parseInt(cl, 10) > MAX_BODY_BYTES) return json({ error: 'payload too large' }, 413);

  const ip = req.headers.get('cf-connecting-ip') ?? 'unknown';
  const rl = await checkRateLimit(env.RATELIMIT, ip);
  if (!rl.allowed) {
    return json({ error: 'rate limited' }, 429, { 'Retry-After': String(rl.retryAfter) });
  }

  const text = await req.text();
  if (text.length > MAX_BODY_BYTES) return json({ error: 'payload too large' }, 413);

  let parsed: unknown;
  try {
    parsed = JSON.parse(text);
  } catch {
    return json({ error: 'malformed json' }, 400);
  }

  const v = validateBundle(parsed);
  if (!v.ok) return json({ error: 'validation failed', details: v.errors }, 400);

  const description = `RimLogging bundle - RW ${v.bundle.rimWorldVersion || '?'} - ${v.bundle.entries.length} entries - ${new Date().toISOString()}`;
  const bundleFile = `bundle-${crypto.randomUUID().slice(0, 8)}.json`;
  const files = {
    'summary.md': { content: renderSummary(v.bundle) },
    [bundleFile]: { content: JSON.stringify(v.bundle, null, 2) },
    'logs.txt': { content: renderLogs(v.bundle) },
  };

  const userToken = req.headers.get('x-gist-token')?.trim();
  const token = userToken || env.GITHUB_TOKEN;
  const g = await createGist(token, { description, public: false, files }, fetchImpl);
  if (!g.ok) return json({ error: 'upstream failed', status: g.status }, 502);

  return json({ url: g.url, id: g.id }, 200);
}

function json(body: unknown, status: number, extraHeaders: Record<string, string> = {}): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json', ...extraHeaders },
  });
}
