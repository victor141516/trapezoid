import { endpoints } from "@trapezoid/common/api";
import { z } from "zod";

let port: number = 8000;
export const setApiPort = (newPort: number) => {
  port = newPort;
};

const buildApiCall =
  <
    Endpoint extends
      | (typeof endpoints.POST)[keyof typeof endpoints.POST]
      | (typeof endpoints.GET)[keyof typeof endpoints.GET],
    Body = z.input<Endpoint["body"]>
  >(
    endpoint: Endpoint
  ) =>
  (
    body: Body extends z.ZodType ? z.input<Body> : undefined = undefined as any
  ): Promise<
    z.input<Endpoint["response"]> extends { ok: true; data: infer Data }
      ? Data
      : // TODO: fix type
        any
  > => {
    return fetch(`http://localhost:${port}${endpoint.url}`, {
      method: endpoint.verb,
      body: endpoint.body ? JSON.stringify(body) : undefined,
      headers: {
        ...(endpoint.body ? { "Content-Type": "application/json" } : {}),
      },
    })
      .then((r) => r.json())
      .then((r) => endpoint.response.parse(r))
      .then((r) => {
        if (r.ok) {
          return r.data as any;
        } else {
          throw new Error(r.error);
        }
      });
  };

// TODO: fix types
export const resume = buildApiCall(endpoints.POST.initialize) as (
  param?: any
) => Promise<any>;
export const pause = buildApiCall(endpoints.POST.stop) as (
  param?: any
) => Promise<any>;
export const setShortcutsActions = buildApiCall(
  endpoints.POST.shortcutsActions
) as (param?: any) => Promise<any>;
export const setOffsets = buildApiCall(endpoints.POST.offsets) as (
  param?: any
) => Promise<any>;
export const getOffsets = buildApiCall(endpoints.GET.offsets) as (
  param?: any
) => Promise<any>;
export const getShortcutsActions = buildApiCall(
  endpoints.GET.shortcutsActions
) as (param?: any) => Promise<any>;
export const getAvailableActions = buildApiCall(
  endpoints.GET.availableActions
) as (param?: any) => Promise<any>;
