import api from "../../../auth/api";
import type { AxiosResponse } from "axios";

export type DashboardDailyVolumePointDto = {
  date: string;
  totalAmount: number;
  transactionCount: number;
};

export type DashboardSummaryDto = {
  totalTenants: number;
  totalOrgUnits: number;
  totalUsers: number;
  totalRemittanceRequests: number;
  totalRemittanceAmount: number;
  dailyRemittanceVolume: DashboardDailyVolumePointDto[];
};

const baseUrl = "/api/admin/dashboard";

export async function fetchDashboardSummary(): Promise<DashboardSummaryDto> {
  const res: AxiosResponse<DashboardSummaryDto> = await api.get(baseUrl);
  return res.data;
}

export const dashboardApi = {
  getSummary: fetchDashboardSummary,
};