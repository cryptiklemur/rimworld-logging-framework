import { defineWorkersConfig } from '@cloudflare/vitest-pool-workers/config';

export default defineWorkersConfig({
  test: {
    poolOptions: {
      workers: {
        wrangler: { configPath: './wrangler.toml' },
        miniflare: {
          kvNamespaces: ['RATELIMIT'],
          bindings: { GITHUB_TOKEN: 'test-token' },
        },
      },
    },
  },
});
