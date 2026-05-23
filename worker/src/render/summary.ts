import type { BundlePayload } from '../types';

export function renderSummary(b: BundlePayload): string {
  const lines: string[] = [];
  lines.push('# RimLogging Bundle');
  lines.push('');
  lines.push(`- **RimWorld version:** ${b.rimWorldVersion || '(unknown)'}`);
  lines.push(`- **Framework version:** ${b.frameworkVersion || '(unknown)'}`);
  lines.push(`- **Captured:** ${new Date().toISOString()}`);
  lines.push('');

  const activeMods = b.mods.filter(m => m.active);
  lines.push(`## Mods (${activeMods.length} active / ${b.mods.length} total)`);
  lines.push('');
  for (const m of activeMods) {
    const ver = m.version ? ` v${m.version}` : '';
    lines.push(`- ${m.name} (\`${m.packageId}\`)${ver}`);
  }
  lines.push('');

  const counts: Record<string, number> = {};
  for (const e of b.entries) counts[e.level] = (counts[e.level] ?? 0) + 1;
  lines.push(`## Entries (${b.entries.length} total)`);
  lines.push('');
  for (const lvl of ['Critical', 'Error', 'Warning', 'Info', 'Debug', 'Trace']) {
    if (counts[lvl]) lines.push(`- ${lvl}: ${counts[lvl]}`);
  }
  lines.push('');

  const errs = b.entries.filter(e => e.level === 'Error' || e.level === 'Critical');
  if (errs.length > 0) {
    lines.push('## Top errors');
    lines.push('');
    const byMsg: Record<string, number> = {};
    for (const e of errs) {
      const k = e.msg.slice(0, 120);
      byMsg[k] = (byMsg[k] ?? 0) + 1;
    }
    const top = Object.entries(byMsg).sort((a, b) => b[1] - a[1]).slice(0, 5);
    for (const [msg, n] of top) lines.push(`- (${n}×) ${msg}`);
    lines.push('');
  }

  return lines.join('\n');
}
