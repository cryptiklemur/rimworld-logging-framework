import type { BundlePayload, ValidationResult } from './types';

export const MAX_ENTRIES = 10000;
export const MAX_MSG_BYTES = 32 * 1024;
export const MAX_BODY_BYTES = 5 * 1024 * 1024;

const LEVELS = new Set(['Trace', 'Debug', 'Info', 'Warning', 'Error', 'Critical']);

export function validateBundle(raw: unknown): ValidationResult {
  const errors: string[] = [];

  if (typeof raw !== 'object' || raw === null || Array.isArray(raw)) {
    return { ok: false, errors: ['root must be an object'] };
  }
  const r = raw as Record<string, unknown>;

  if (typeof r.rimWorldVersion !== 'string') errors.push('rimWorldVersion must be a string');
  if (typeof r.frameworkVersion !== 'string') errors.push('frameworkVersion must be a string');

  if (Array.isArray(r.mods)) {
    r.mods.forEach((m, i) => validateMod(m, i, errors));
  } else {
    errors.push('mods must be an array');
  }

  if (Array.isArray(r.entries)) {
    if (r.entries.length > MAX_ENTRIES) errors.push(`entries exceeds max of ${MAX_ENTRIES}`);
    r.entries.forEach((e, i) => validateEntry(e, i, errors));
  } else {
    errors.push('entries must be an array');
  }

  if (errors.length > 0) return { ok: false, errors };
  return { ok: true, bundle: raw as BundlePayload };
}

function validateMod(m: unknown, i: number, errors: string[]): void {
  if (typeof m !== 'object' || m === null || Array.isArray(m)) {
    errors.push(`mods[${i}] must be an object`);
    return;
  }
  const mm = m as Record<string, unknown>;
  if (typeof mm.name !== 'string') errors.push(`mods[${i}].name must be a string`);
  if (typeof mm.packageId !== 'string') errors.push(`mods[${i}].packageId must be a string`);
  if (typeof mm.active !== 'boolean') errors.push(`mods[${i}].active must be a boolean`);
  if (mm.version !== undefined && mm.version !== null && typeof mm.version !== 'string') {
    errors.push(`mods[${i}].version must be a string or null`);
  }
}

function validateEntry(e: unknown, i: number, errors: string[]): void {
  if (typeof e !== 'object' || e === null || Array.isArray(e)) {
    errors.push(`entries[${i}] must be an object`);
    return;
  }
  const ee = e as Record<string, unknown>;
  if (typeof ee.ts !== 'string') errors.push(`entries[${i}].ts must be a string`);
  if (typeof ee.level !== 'string') {
    errors.push(`entries[${i}].level must be a string`);
  } else if (!LEVELS.has(ee.level)) {
    errors.push(`entries[${i}].level "${ee.level}" not recognized`);
  }
  if (typeof ee.channel !== 'string') errors.push(`entries[${i}].channel must be a string`);
  if (typeof ee.source !== 'string') errors.push(`entries[${i}].source must be a string`);
  if (typeof ee.msg !== 'string') {
    errors.push(`entries[${i}].msg must be a string`);
  } else if (utf8Bytes(ee.msg) > MAX_MSG_BYTES) {
    errors.push(`entries[${i}].msg exceeds max of ${MAX_MSG_BYTES} bytes`);
  }
  if (ee.stack !== undefined && ee.stack !== null && typeof ee.stack !== 'string') {
    errors.push(`entries[${i}].stack must be a string or null`);
  }
  if (ee.ctx !== undefined && ee.ctx !== null && (typeof ee.ctx !== 'object' || Array.isArray(ee.ctx))) {
    errors.push(`entries[${i}].ctx must be an object or null`);
  }
}

function utf8Bytes(s: string): number {
  return new TextEncoder().encode(s).length;
}
