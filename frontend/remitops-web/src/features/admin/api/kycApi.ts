import api from "../../../auth/api";

export type AdminKycReviewDto = {
  id: string;

  tenantId: string;
  tenantName: string;

  orgUnitId: string;
  orgUnitName: string;

  identityUserId: string;
  applicantName: string;
  applicantEmail: string;

  documentType: string;
  fileName: string;
  status: string;

  submittedAtUtc?: string | null;
};

export const kycApi = {
  async list(): Promise<AdminKycReviewDto[]> {
    const response = await api.get<AdminKycReviewDto[]>("/api/admin/kyc");
    return response.data;
  },
};