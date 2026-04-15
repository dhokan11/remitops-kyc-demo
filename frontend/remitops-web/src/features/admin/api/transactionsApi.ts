import api from "../../../auth/api";
import type { AxiosResponse } from "axios";

export type AdminTransactionDto = {
  id: string;
  tenantId: string;
  tenantName: string;
  sourceOrgUnitId: string;
  sourceOrgUnitName: string;
  destinationOrgUnitId: string;
  destinationOrgUnitName: string;
  submittedByUserId: string;
  beneficiaryName: string | null;
  beneficiaryCountryCode: string | null;
  beneficiaryCity: string | null;
  amount: number;
  currency: string;
  currentQueue: string;
  currentStatus: string;
  lastActionByUserId: string | null;
  priority: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  reference: string;
};

const baseUrl = "/api/admin/transactions";

export const transactionsApi = {
  async list(): Promise<AdminTransactionDto[]> {
    const res: AxiosResponse<AdminTransactionDto[]> = await api.get(baseUrl);
    return res.data;
  },

  async getById(id: string): Promise<AdminTransactionDto> {
    const res: AxiosResponse<AdminTransactionDto> = await api.get(`${baseUrl}/${id}`);
    return res.data;
  },
};