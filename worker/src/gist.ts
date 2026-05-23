export type GistFile = { content: string };

export type GistRequest = {
  description: string;
  public: boolean;
  files: Record<string, GistFile>;
};

export type GistOk = { ok: true; url: string; id: string };
export type GistErr = { ok: false; status: number; body: string };
export type GistResult = GistOk | GistErr;

export type FetchLike = (input: string, init: RequestInit) => Promise<Response>;

export async function createGist(
  token: string,
  req: GistRequest,
  fetchImpl: FetchLike = fetch,
): Promise<GistResult> {
  const resp = await fetchImpl('https://api.github.com/gists', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Accept': 'application/vnd.github+json',
      'X-GitHub-Api-Version': '2022-11-28',
      'User-Agent': 'rimlogging-bundle-worker',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(req),
  });
  const body = await resp.text();
  if (!resp.ok) return { ok: false, status: resp.status, body };
  try {
    const j = JSON.parse(body) as { html_url?: string; id?: string };
    if (!j.html_url || !j.id) return { ok: false, status: resp.status, body: 'missing html_url or id in github response' };
    return { ok: true, url: j.html_url, id: j.id };
  } catch {
    return { ok: false, status: resp.status, body: 'malformed JSON from github' };
  }
}
