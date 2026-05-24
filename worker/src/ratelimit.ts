export const RATE_LIMIT_PER_WINDOW = 2;
export const RATE_WINDOW_SECONDS = 60;

export type RateLimitResult =
  | { allowed: true }
  | { allowed: false; retryAfter: number };

type State = { count: number; resetAt: number };

export async function checkRateLimit(
  kv: KVNamespace,
  ip: string,
  now: number = Date.now(),
): Promise<RateLimitResult> {
  const nowSec = Math.floor(now / 1000);
  const key = `rl:${ip}`;
  const raw = await kv.get(key);
  const state: State = raw
    ? (JSON.parse(raw) as State)
    : { count: 0, resetAt: nowSec + RATE_WINDOW_SECONDS };

  if (state.count >= RATE_LIMIT_PER_WINDOW) {
    const retryAfter = Math.max(1, state.resetAt - nowSec);
    return { allowed: false, retryAfter };
  }

  state.count += 1;
  const ttl = Math.max(1, state.resetAt - nowSec);
  await kv.put(key, JSON.stringify(state), { expirationTtl: ttl });
  return { allowed: true };
}
