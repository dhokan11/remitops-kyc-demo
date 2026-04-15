import api from "../../../auth/api";
import type { AxiosResponse } from "axios";

export type TenantDto = {
  id: string;
  code: string;
  name: string;
  countryCode: string | null;
  city: string | null;
  latitude: number | null;
  longitude: number | null;
  isActive: boolean;
  status: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
};

export type UpsertTenantRequest = {
  code: string;
  name: string;
  countryCode?: string | null;
  city?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  isActive: boolean;
};

const baseUrl = "/api/admin/tenants";

export const tenantsApi = {
  async list(): Promise<TenantDto[]> {
    const res: AxiosResponse<TenantDto[]> = await api.get(baseUrl);
    return res.data;
  },

  async create(payload: UpsertTenantRequest): Promise<TenantDto> {
    const res: AxiosResponse<TenantDto> = await api.post(baseUrl, payload);
    return res.data;
  },

  async update(id: string, payload: UpsertTenantRequest): Promise<void> {
    await api.put(`${baseUrl}/${id}`, payload);
  },

  async toggleStatus(id: string, isActive: boolean): Promise<TenantDto> {
    const res: AxiosResponse<TenantDto> = await api.patch(`${baseUrl}/${id}/status`, {
      isActive,
    });
    return res.data;
  },
};