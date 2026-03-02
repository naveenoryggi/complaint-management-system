import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface OEMMaster {
  id: string;
  tenant_id: string;
  name: string;
  country?: string;
  is_indian: boolean;
  india_distributor?: string;
  partner_tier?: string;
  product_categories?: string[];
  created_at: string;
}

export interface OEMTenderRequirement {
  id: string;
  tender_id: string;
  oem_id: string;
  maf_status: string;
  ms_status: string;
  partner_cert_status: string;
  datasheet_status: string;
  compliance_cert_status: string;
  notes?: string;
  created_at: string;
}

export interface PortalRegistration {
  id: string;
  portal_name: string;
  portal_url?: string;
  portal_type?: string;
  registration_status: string;
  registration_number?: string;
  dsc_class?: string;
  dsc_holder_name?: string;
  dsc_expiry_date?: string;
  created_at: string;
}

export interface EMDRecord {
  id: string;
  tender_id: string;
  amount: number;
  mode: string;
  instrument_number?: string;
  instrument_date?: string;
  issuing_bank?: string;
  validity_start_date?: string;
  validity_end_date?: string;
  submitted_date?: string;
  released_date?: string;
  release_amount?: number;
  status: string;
  notes?: string;
  created_at: string;
  updated_at: string;
}

export interface TenderFee {
  id: string;
  tender_id: string;
  fee_type: string;
  amount: number;
  payment_mode?: string;
  payment_reference?: string;
  payment_date?: string;
  receipt_path?: string;
  status: string;
  notes?: string;
  created_at: string;
  updated_at: string;
}

export interface DashboardSummary {
  active_tenders: number;
  total_bundles: number;
  total_certifications: number;
  active_portals: number;
  total_oems: number;
  total_emd_amount: number;
}

@Injectable({ providedIn: 'root' })
export class TrackingService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/tracking`;

  getDashboard(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.baseUrl}/dashboard`);
  }
  listOEMs(): Observable<OEMMaster[]> {
    return this.http.get<OEMMaster[]>(`${this.baseUrl}/oem`);
  }
  createOEM(data: Partial<OEMMaster>): Observable<OEMMaster> {
    return this.http.post<OEMMaster>(`${this.baseUrl}/oem`, data);
  }
  updateOEM(id: string, data: Partial<OEMMaster>): Observable<OEMMaster> {
    return this.http.put<OEMMaster>(`${this.baseUrl}/oem/${id}`, data);
  }
  deleteOEM(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/oem/${id}`);
  }
  listOEMRequirements(tenderId: string): Observable<OEMTenderRequirement[]> {
    return this.http.get<OEMTenderRequirement[]>(`${this.baseUrl}/oem-requirements/tender/${tenderId}`);
  }
  createOEMRequirement(tenderId: string, data: Partial<OEMTenderRequirement>): Observable<OEMTenderRequirement> {
    return this.http.post<OEMTenderRequirement>(`${this.baseUrl}/oem-requirements/tender/${tenderId}`, data);
  }
  updateOEMRequirement(reqId: string, data: Partial<OEMTenderRequirement>): Observable<OEMTenderRequirement> {
    return this.http.put<OEMTenderRequirement>(`${this.baseUrl}/oem-requirements/${reqId}`, data);
  }
  deleteOEMRequirement(reqId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/oem-requirements/${reqId}`);
  }
  listPortals(): Observable<PortalRegistration[]> {
    return this.http.get<PortalRegistration[]>(`${this.baseUrl}/portals`);
  }
  createPortal(data: Partial<PortalRegistration>): Observable<PortalRegistration> {
    return this.http.post<PortalRegistration>(`${this.baseUrl}/portals`, data);
  }
  updatePortal(id: string, data: Partial<PortalRegistration>): Observable<PortalRegistration> {
    return this.http.put<PortalRegistration>(`${this.baseUrl}/portals/${id}`, data);
  }
  deletePortal(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/portals/${id}`);
  }
  seedIndianPortals(): Observable<any> {
    return this.http.post(`${this.baseUrl}/portals/seed-indian-portals`, {});
  }
  listEMDs(tenderId?: string): Observable<EMDRecord[]> {
    let params = new HttpParams();
    if (tenderId) params = params.set('tender_id', tenderId);
    return this.http.get<EMDRecord[]>(`${this.baseUrl}/emd`, { params });
  }
  createEMD(tenderId: string, data: Partial<EMDRecord>): Observable<EMDRecord> {
    return this.http.post<EMDRecord>(`${this.baseUrl}/emd/tender/${tenderId}`, data);
  }
  updateEMD(emdId: string, data: Partial<EMDRecord>): Observable<EMDRecord> {
    return this.http.put<EMDRecord>(`${this.baseUrl}/emd/${emdId}`, data);
  }
  deleteEMD(emdId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/emd/${emdId}`);
  }
  listFees(tenderId?: string): Observable<TenderFee[]> {
    let params = new HttpParams();
    if (tenderId) params = params.set('tender_id', tenderId);
    return this.http.get<TenderFee[]>(`${this.baseUrl}/fees`, { params });
  }
  createFee(tenderId: string, data: Partial<TenderFee>): Observable<TenderFee> {
    return this.http.post<TenderFee>(`${this.baseUrl}/fees/tender/${tenderId}`, data);
  }
  updateFee(feeId: string, data: Partial<TenderFee>): Observable<TenderFee> {
    return this.http.put<TenderFee>(`${this.baseUrl}/fees/${feeId}`, data);
  }
  deleteFee(feeId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/fees/${feeId}`);
  }
}
