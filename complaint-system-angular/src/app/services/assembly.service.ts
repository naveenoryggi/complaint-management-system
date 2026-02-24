import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface MergePDFRequest {
  document_ids: string[];
  add_cover: boolean;
  cover_data?: CoverPageData;
  output_filename?: string;
}

export interface CoverPageData {
  tender_title: string;
  tender_reference?: string;
  issuing_authority: string;
  company_name: string;
  company_address?: string;
  company_contact?: string;
  company_email?: string;
  submission_date?: string;
  company_logo?: string;
}

export interface ReorderPagesRequest {
  document_id: string;
  page_order: number[];
  output_filename?: string;
}

export interface RemovePagesRequest {
  document_id: string;
  pages_to_remove: number[];
  output_filename?: string;
}

export interface ExportPackageRequest {
  document_ids: string[];
  package_name: string;
  include_cover: boolean;
  cover_data?: CoverPageData;
  merge_pdfs: boolean;
}

export interface AssemblyResponse {
  success: boolean;
  file_path: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class AssemblyService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/assembly`;

  /**
   * Merge multiple PDFs into one
   */
  mergePDFs(request: MergePDFRequest): Observable<AssemblyResponse> {
    return this.http.post<AssemblyResponse>(`${this.baseUrl}/merge`, request);
  }

  /**
   * Reorder pages in a PDF
   */
  reorderPages(request: ReorderPagesRequest): Observable<AssemblyResponse> {
    return this.http.post<AssemblyResponse>(`${this.baseUrl}/reorder`, request);
  }

  /**
   * Remove pages from a PDF
   */
  removePages(request: RemovePagesRequest): Observable<AssemblyResponse> {
    return this.http.post<AssemblyResponse>(`${this.baseUrl}/remove-pages`, request);
  }

  /**
   * Export tender package as ZIP
   */
  exportPackage(request: ExportPackageRequest): Observable<AssemblyResponse> {
    return this.http.post<AssemblyResponse>(`${this.baseUrl}/export-package`, request);
  }

  /**
   * Generate standalone cover page
   */
  generateCoverPage(coverData: CoverPageData): Observable<AssemblyResponse> {
    return this.http.post<AssemblyResponse>(`${this.baseUrl}/generate-cover`, coverData);
  }

  /**
   * Get download URL for assembled file
   */
  getDownloadUrl(fileType: 'assembled' | 'packages', filename: string): string {
    return `${this.baseUrl}/download/${fileType}/${filename}`;
  }

  /**
   * Download assembled file
   */
  downloadFile(fileType: 'assembled' | 'packages', filename: string): Observable<Blob> {
    return this.http.get(this.getDownloadUrl(fileType, filename), {
      responseType: 'blob'
    });
  }
}
