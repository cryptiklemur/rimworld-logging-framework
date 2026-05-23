import { env } from 'cloudflare:test';
import { beforeEach, describe, expect, it } from 'vitest';
import { checkRateLimit, RATE_LIMIT_PER_WINDOW } from '../src/ratelimit';

describe('checkRateLimit', () => {
  beforeEach(async () => {
    const list = await env.RATELIMIT.list();
    for (const k of list.keys) await env.RATELIMIT.delete(k.name);
  });

  it('allows the first request from a fresh IP', async () => {
    const r = await checkRateLimit(env.RATELIMIT, '1.2.3.4');
    expect(r.allowed).toBe(true);
  });

  it('allows up to RATE_LIMIT_PER_WINDOW requests', async () => {
    for (let i = 0; i < RATE_LIMIT_PER_WINDOW; i++) {
      const r = await checkRateLimit(env.RATELIMIT, '5.6.7.8');
      expect(r.allowed).toBe(true);
    }
  });

  it('blocks the next request after the limit', async () => {
    for (let i = 0; i < RATE_LIMIT_PER_WINDOW; i++) {
      await checkRateLimit(env.RATELIMIT, '9.9.9.9');
    }
    const r = await checkRateLimit(env.RATELIMIT, '9.9.9.9');
    expect(r.allowed).toBe(false);
    if (!r.allowed) expect(r.retryAfter).toBeGreaterThan(0);
  });

  it('tracks IPs independently', async () => {
    for (let i = 0; i < RATE_LIMIT_PER_WINDOW; i++) {
      await checkRateLimit(env.RATELIMIT, 'a');
    }
    const blocked = await checkRateLimit(env.RATELIMIT, 'a');
    const allowed = await checkRateLimit(env.RATELIMIT, 'b');
    expect(blocked.allowed).toBe(false);
    expect(allowed.allowed).toBe(true);
  });
});
