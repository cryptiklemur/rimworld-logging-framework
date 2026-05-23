import type { BundlePayload } from '../types';

export function renderLogs(b: BundlePayload): string {
  const lines: string[] = [];
  for (const e of b.entries) {
    lines.push(`[${e.ts}] ${e.level.toUpperCase()} ${e.channel}: ${e.msg}`);
    if (e.stack) {
      for (const sl of e.stack.split('\n')) {
        lines.push(`    ${sl}`);
      }
    }
  }
  return lines.join('\n');
}
