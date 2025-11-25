export interface Company {
  id: string;
  name: string;
  code: string;
  description?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  logoUrl?: string;
  logoFileName?: string;
  isActive: boolean;
}

export interface UpdateCompanyRequest {
  name: string;
  description?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
}

export interface CompanyResponse {
  isSuccess: boolean;
  message?: string;
  data?: Company;
}

export interface UploadLogoResponse {
  isSuccess: boolean;
  message?: string;
  data?: {
    logoUrl: string;
    fileName: string;
    contentType: string;
  };
}
