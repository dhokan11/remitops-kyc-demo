// ~/projects/remitops/frontend/remitops-web/src/utils/logger.ts
const enabled = true;

function ts() {
  return new Date().toISOString();
}

export const log = {
  info: (tag: string, data?: unknown) => {
    if (!enabled) return;
    console.log(`[FE][INFO][${ts()}][${tag}]`, data ?? "");
  },
  warn: (tag: string, data?: unknown) => {
    if (!enabled) return;
    console.warn(`[FE][WARN][${ts()}][${tag}]`, data ?? "");
  },
  error: (tag: string, data?: unknown) => {
    if (!enabled) return;
    console.error(`[FE][ERROR][${ts()}][${tag}]`, data ?? "");
  },
  trace: (tag: string, data?: unknown) => {
    if (!enabled) return;
    console.debug(`[FE][TRACE][${ts()}][${tag}]`, data ?? "");
  },
};