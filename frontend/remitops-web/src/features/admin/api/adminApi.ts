import { log } from "../../../utils/logger";

const API =
  import.meta.env.VITE_API_BASE_URL && import.meta.env.VITE_API_BASE_URL.trim() !== ""
    ? import.meta.env.VITE_API_BASE_URL.replace(/\/+$/, "") // strip trailing slash
    : ""; // empty string => use Vite dev server proxy for /api

function getToken(): string {
  const rawToken = localStorage.getItem("remitops_token");
  const rawAuthUser = localStorage.getItem("auth_user");

  log.trace("authHeaders.storedValues", {
    remitops_token_present: !!rawToken,
    auth_user_present: !!rawAuthUser,
  });

  if (rawToken) return rawToken;

  if (rawAuthUser) {
    try {
      const user = JSON.parse(rawAuthUser);
      return user?.token || "";
    } catch (err) {
      log.warn("authHeaders.parseAuthUserFailed", { err, rawAuthUser });
    }
  }

  return "";
}

function authHeaders(): HeadersInit {
  const token = getToken();
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function http<T>(url: string, options?: RequestInit): Promise<T> {
  const fullUrl = `${API}${url}`;
  log.info("http.request", { API, url, fullUrl, options });

  let res: Response;
  try {
    res = await fetch(fullUrl, {
      ...options,
      headers: { ...authHeaders(), ...(options?.headers || {}) },
    });
  } catch (networkErr) {
    log.error("http.networkError", { fullUrl, networkErr });
    throw networkErr;
  }

  const headersObj = Object.fromEntries(res.headers.entries());
  const text = await res.text();

  log.info("http.response", {
    fullUrl,
    status: res.status,
    statusText: res.statusText,
    headers: headersObj,
    bodyPreview: text.slice(0, 400),
  });

  if (!res.ok) {
    const err = new Error(
      `HTTP ${res.status} ${res.statusText} for ${fullUrl} :: ${text}`,
    );
    (err as any).status = res.status;
    (err as any).body = text;
    log.error("http.error", err);
    throw err;
  }

  if (!text) {
    log.trace("http.emptyBody", fullUrl);
    return null as T;
  }

  try {
    const json = JSON.parse(text) as T;
    log.trace("http.parsedJson", { fullUrl, jsonSample: json });
    return json;
  } catch (parseErr) {
    log.error("http.parseError", { fullUrl, parseErr, raw: text });
    throw parseErr;
  }
}

export const adminApi = {
  getTenants: () => http<any[]>("/api/admin/tenants"),
  createTenant: (body: any) =>
    http<any>("/api/admin/tenants", {
      method: "POST",
      body: JSON.stringify(body),
    }),
  updateTenant: (id: number, body: any) =>
    http<any>(`/api/admin/tenants/${id}`, {
      method: "PUT",
      body: JSON.stringify(body),
    }),
  getOrgUnits: () => http<any[]>("/api/admin/org-units"),
  createOrgUnit: (body: any) =>
    http<any>("/api/admin/org-units", {
      method: "POST",
      body: JSON.stringify(body),
    }),
  updateOrgUnit: (id: number, body: any) =>
    http<any>(`/api/admin/org-units/${id}`, {
      method: "PUT",
      body: JSON.stringify(body),
    }),
  getUsers: () => http<any[]>("/api/admin/users"),
  createUser: (body: any) =>
    http<any>("/api/admin/users", {
      method: "POST",
      body: JSON.stringify(body),
    }),
  updateUser: (id: number, body: any) =>
    http<any>(`/api/admin/users/${id}`, {
      method: "PUT",
      body: JSON.stringify(body),
    }),
  getAuditTrail: () => http<any[]>("/api/admin/audit-trail"),
};