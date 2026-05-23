import { describe, expect, it } from 'vitest';
import { MAX_ENTRIES, MAX_MSG_BYTES, validateBundle } from '../src/validate';

const minimalBundle = () => ({
  rimWorldVersion: '1.5.4297',
  frameworkVersion: '1.0.0-beta.1',
  mods: [],
  entries: [],
});

describe('validateBundle', () => {
  it('accepts a minimal valid bundle', () => {
    const r = validateBundle(minimalBundle());
    expect(r.ok).toBe(true);
  });

  it('rejects non-object root', () => {
    const r = validateBundle('nope');
    expect(r.ok).toBe(false);
    if (!r.ok) expect(r.errors[0]).toContain('root');
  });

  it('rejects missing rimWorldVersion', () => {
    const b = minimalBundle() as any;
    delete b.rimWorldVersion;
    const r = validateBundle(b);
    expect(r.ok).toBe(false);
    if (!r.ok) expect(r.errors.join('|')).toContain('rimWorldVersion');
  });

  it('rejects non-array mods', () => {
    const b = minimalBundle() as any;
    b.mods = 'not an array';
    const r = validateBundle(b);
    expect(r.ok).toBe(false);
    if (!r.ok) expect(r.errors.join('|')).toContain('mods');
  });

  it('rejects mod missing required field', () => {
    const b = minimalBundle();
    b.mods = [{ name: 'X', active: true }] as any;
    const r = validateBundle(b);
    expect(r.ok).toBe(false);
    if (!r.ok) expect(r.errors.join('|')).toContain('mods[0].packageId');
  });

  it('accepts mod with null version', () => {
    const b = minimalBundle();
    b.mods = [{ name: 'X', packageId: 'a.b', version: null, active: true }];
    const r = validateBundle(b);
    expect(r.ok).toBe(true);
  });

  it('rejects entry with unknown level', () => {
    const b = minimalBundle();
    b.entries = [{ ts: 't', level: 'Bogus', channel: 'c', source: '', msg: 'm' }];
    const r = validateBundle(b);
    expect(r.ok).toBe(false);
    if (!r.ok) expect(r.errors.join('|')).toContain('level');
  });

  it('accepts entry with Critical level', () => {
    const b = minimalBundle();
    b.entries = [{ ts: 't', level: 'Critical', channel: 'c', source: '', msg: 'm' }];
    const r = validateBundle(b);
    expect(r.ok).toBe(true);
  });

  it('rejects when entries exceeds MAX_ENTRIES', () => {
    const b = minimalBundle();
    b.entries = new Array(MAX_ENTRIES + 1).fill(0).map((_, i) => ({
      ts: 't', level: 'Info', channel: 'c', source: '', msg: `m${i}`,
    }));
    const r = validateBundle(b);
    expect(r.ok).toBe(false);
    if (!r.ok) expect(r.errors.join('|')).toContain('entries exceeds');
  });

  it('rejects msg larger than MAX_MSG_BYTES', () => {
    const b = minimalBundle();
    b.entries = [{ ts: 't', level: 'Info', channel: 'c', source: '', msg: 'a'.repeat(MAX_MSG_BYTES + 1) }];
    const r = validateBundle(b);
    expect(r.ok).toBe(false);
    if (!r.ok) expect(r.errors.join('|')).toContain('exceeds max');
  });

  it('rejects ctx that is an array', () => {
    const b = minimalBundle();
    b.entries = [{ ts: 't', level: 'Info', channel: 'c', source: '', msg: 'm', ctx: [1, 2] as any }];
    const r = validateBundle(b);
    expect(r.ok).toBe(false);
    if (!r.ok) expect(r.errors.join('|')).toContain('ctx');
  });

  it('accepts entry with null stack', () => {
    const b = minimalBundle();
    b.entries = [{ ts: 't', level: 'Info', channel: 'c', source: '', msg: 'm', stack: null }];
    const r = validateBundle(b);
    expect(r.ok).toBe(true);
  });
});
