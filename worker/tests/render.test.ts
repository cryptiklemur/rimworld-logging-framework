import { describe, expect, it } from 'vitest';
import { renderLogs } from '../src/render/logs';
import { renderSummary } from '../src/render/summary';
import type { BundlePayload } from '../src/types';

const bundle = (): BundlePayload => ({
  rimWorldVersion: '1.5.4297',
  frameworkVersion: '1.0.0-beta.1',
  mods: [
    { name: 'Core', packageId: 'ludeon.rimworld', active: true, version: '1.5' },
    { name: 'Disabled Mod', packageId: 'a.b', active: false, version: '0.1' },
  ],
  entries: [
    { ts: '2026-05-23T10:00:00Z', level: 'Info', channel: 'core', source: '', msg: 'starting' },
    { ts: '2026-05-23T10:00:01Z', level: 'Error', channel: 'core', source: 'F.cs:10', msg: 'boom' },
    { ts: '2026-05-23T10:00:02Z', level: 'Error', channel: 'core', source: 'F.cs:11', msg: 'boom' },
    { ts: '2026-05-23T10:00:03Z', level: 'Warning', channel: 'core', source: '', msg: 'odd' },
  ],
});

describe('renderSummary', () => {
  it('includes versions and entry counts', () => {
    const md = renderSummary(bundle());
    expect(md).toContain('1.5.4297');
    expect(md).toContain('1.0.0-beta.1');
    expect(md).toContain('Info: 1');
    expect(md).toContain('Error: 2');
    expect(md).toContain('Warning: 1');
  });

  it('lists only active mods in the mods section', () => {
    const md = renderSummary(bundle());
    expect(md).toContain('Core');
    expect(md).toContain('1 active / 2 total');
    expect(md).not.toMatch(/^- Disabled Mod/m);
  });

  it('groups top errors with multiplicity', () => {
    const md = renderSummary(bundle());
    expect(md).toContain('Top errors');
    expect(md).toContain('(2×) boom');
  });

  it('omits top errors section when none present', () => {
    const b = bundle();
    b.entries = b.entries.filter(e => e.level !== 'Error');
    const md = renderSummary(b);
    expect(md).not.toContain('Top errors');
  });
});

describe('renderLogs', () => {
  it('emits one line per entry', () => {
    const txt = renderLogs(bundle());
    const lines = txt.split('\n').filter(l => !l.startsWith('    '));
    expect(lines).toHaveLength(4);
  });

  it('formats each line with timestamp, level, channel, and message', () => {
    const txt = renderLogs(bundle());
    expect(txt).toContain('[2026-05-23T10:00:00Z] INFO core: starting');
  });

  it('indents stack lines by 4 spaces', () => {
    const b = bundle();
    b.entries = [{ ts: 't', level: 'Error', channel: 'c', source: '', msg: 'x', stack: 'frame1\nframe2' }];
    const txt = renderLogs(b);
    expect(txt).toContain('    frame1');
    expect(txt).toContain('    frame2');
  });
});
