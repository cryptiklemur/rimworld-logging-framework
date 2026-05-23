export type BundlePayload = {
  rimWorldVersion: string;
  frameworkVersion: string;
  mods: ModInfo[];
  entries: EntryDto[];
};

export type ModInfo = {
  name: string;
  packageId: string;
  version?: string | null;
  active: boolean;
};

export type EntryDto = {
  ts: string;
  level: string;
  channel: string;
  source: string;
  msg: string;
  ctx?: Record<string, unknown> | null;
  stack?: string | null;
};

export type ValidationOk = { ok: true; bundle: BundlePayload };
export type ValidationErr = { ok: false; errors: string[] };
export type ValidationResult = ValidationOk | ValidationErr;
