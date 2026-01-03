import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Product,
  ProductSummary,
  CreateProductRequest,
  UpdateProductRequest,
  ProductLookup,
  ProductCategory,
  CategoryTreeNode,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  CategoryLookup,
  ProductPriceList,
  CreatePriceListRequest,
  UpdatePriceListRequest,
  CalculatePriceRequest,
  CalculatePriceResponse,
  ProductStatistics
} from '../models/product.model';

interface ApiResponse<T> {
  isSuccess: boolean;
  message?: string;
  data: T;
  errors?: string[];
}

interface PagedResponse<T> {
  isSuccess: boolean;
  message?: string;
  data: {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  };
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly baseUrl = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) {}

  // ==================== CATEGORY OPERATIONS ====================

  getCategoryTree(): Observable<ApiResponse<CategoryTreeNode[]>> {
    return this.http.get<ApiResponse<CategoryTreeNode[]>>(`${this.baseUrl}/categories/tree`);
  }

  getCategories(params?: {
    search?: string;
    parentId?: string;
    isActive?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<PagedResponse<ProductCategory>> {
    let httpParams = new HttpParams();
    if (params?.search) httpParams = httpParams.set('searchTerm', params.search);
    if (params?.parentId) httpParams = httpParams.set('parentId', params.parentId);
    if (params?.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
    if (params?.pageNumber) httpParams = httpParams.set('page', params.pageNumber.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PagedResponse<ProductCategory>>(`${this.baseUrl}/categories`, { params: httpParams });
  }

  getCategoryById(id: string): Observable<ApiResponse<ProductCategory>> {
    return this.http.get<ApiResponse<ProductCategory>>(`${this.baseUrl}/categories/${id}`);
  }

  getCategoryByCode(code: string): Observable<ApiResponse<ProductCategory>> {
    return this.http.get<ApiResponse<ProductCategory>>(`${this.baseUrl}/categories/by-code/${code}`);
  }

  createCategory(request: CreateCategoryRequest): Observable<ApiResponse<ProductCategory>> {
    return this.http.post<ApiResponse<ProductCategory>>(`${this.baseUrl}/categories`, request);
  }

  updateCategory(id: string, request: UpdateCategoryRequest): Observable<ApiResponse<ProductCategory>> {
    return this.http.put<ApiResponse<ProductCategory>>(`${this.baseUrl}/categories/${id}`, request);
  }

  moveCategory(id: string, newParentId: string | null): Observable<ApiResponse<ProductCategory>> {
    return this.http.post<ApiResponse<ProductCategory>>(`${this.baseUrl}/categories/${id}/move`, { newParentId });
  }

  deleteCategory(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/categories/${id}`);
  }

  getCategoryLookup(search?: string): Observable<ApiResponse<CategoryLookup[]>> {
    let params = new HttpParams();
    if (search) params = params.set('searchTerm', search);
    return this.http.get<ApiResponse<CategoryLookup[]>>(`${this.baseUrl}/categories/lookup`, { params });
  }

  // ==================== PRODUCT OPERATIONS ====================

  getProducts(params?: {
    search?: string;
    categoryId?: string;
    type?: number;
    status?: number;
    inStock?: boolean;
    isFeatured?: boolean;
    isPublic?: boolean;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: string;
  }): Observable<PagedResponse<ProductSummary>> {
    let httpParams = new HttpParams();
    if (params?.search) httpParams = httpParams.set('searchTerm', params.search);
    if (params?.categoryId) httpParams = httpParams.set('categoryId', params.categoryId);
    if (params?.type !== undefined) httpParams = httpParams.set('type', params.type.toString());
    if (params?.status !== undefined) httpParams = httpParams.set('status', params.status.toString());
    if (params?.inStock !== undefined) httpParams = httpParams.set('inStock', params.inStock.toString());
    if (params?.isFeatured !== undefined) httpParams = httpParams.set('isFeatured', params.isFeatured.toString());
    if (params?.isPublic !== undefined) httpParams = httpParams.set('isPublic', params.isPublic.toString());
    if (params?.pageNumber) httpParams = httpParams.set('page', params.pageNumber.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params?.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params?.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);

    return this.http.get<PagedResponse<ProductSummary>>(this.baseUrl, { params: httpParams });
  }

  getProductById(id: string): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.baseUrl}/${id}`);
  }

  getProductByCode(code: string): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.baseUrl}/by-code/${code}`);
  }

  getProductBySKU(sku: string): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.baseUrl}/by-sku/${sku}`);
  }

  getProductsByCategory(categoryId: string, params?: {
    pageNumber?: number;
    pageSize?: number;
  }): Observable<PagedResponse<ProductSummary>> {
    let httpParams = new HttpParams();
    if (params?.pageNumber) httpParams = httpParams.set('page', params.pageNumber.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PagedResponse<ProductSummary>>(`${this.baseUrl}/by-category/${categoryId}`, { params: httpParams });
  }

  getProductVariants(productId: string): Observable<ApiResponse<ProductSummary[]>> {
    return this.http.get<ApiResponse<ProductSummary[]>>(`${this.baseUrl}/${productId}/variants`);
  }

  createProduct(request: CreateProductRequest): Observable<ApiResponse<Product>> {
    return this.http.post<ApiResponse<Product>>(this.baseUrl, request);
  }

  updateProduct(id: string, request: UpdateProductRequest): Observable<ApiResponse<Product>> {
    return this.http.put<ApiResponse<Product>>(`${this.baseUrl}/${id}`, request);
  }

  deleteProduct(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }

  cloneProduct(id: string, newCode?: string): Observable<ApiResponse<Product>> {
    return this.http.post<ApiResponse<Product>>(`${this.baseUrl}/${id}/clone`, { newCode });
  }

  // NOTE: updateInventory has been removed.
  // Inventory quantities are now managed through the Stock Management module.
  // Use StockService.createStockMovement() for all inventory adjustments.

  getProductLookup(search?: string, categoryId?: string): Observable<ApiResponse<ProductLookup[]>> {
    let params = new HttpParams();
    if (search) params = params.set('searchTerm', search);
    if (categoryId) params = params.set('categoryId', categoryId);
    return this.http.get<ApiResponse<ProductLookup[]>>(`${this.baseUrl}/lookup`, { params });
  }

  // ==================== PRICE LIST OPERATIONS ====================

  getProductPriceLists(productId: string): Observable<ApiResponse<ProductPriceList[]>> {
    return this.http.get<ApiResponse<ProductPriceList[]>>(`${this.baseUrl}/${productId}/price-lists`);
  }

  getPriceListById(id: string): Observable<ApiResponse<ProductPriceList>> {
    return this.http.get<ApiResponse<ProductPriceList>>(`${this.baseUrl}/price-lists/${id}`);
  }

  createPriceList(request: CreatePriceListRequest): Observable<ApiResponse<ProductPriceList>> {
    return this.http.post<ApiResponse<ProductPriceList>>(`${this.baseUrl}/price-lists`, request);
  }

  updatePriceList(id: string, request: UpdatePriceListRequest): Observable<ApiResponse<ProductPriceList>> {
    return this.http.put<ApiResponse<ProductPriceList>>(`${this.baseUrl}/price-lists/${id}`, request);
  }

  deletePriceList(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/price-lists/${id}`);
  }

  calculatePrice(request: CalculatePriceRequest): Observable<ApiResponse<CalculatePriceResponse>> {
    return this.http.post<ApiResponse<CalculatePriceResponse>>(`${this.baseUrl}/calculate-price`, request);
  }

  bulkUpdatePrices(updates: { productId: string; unitPrice: number }[]): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/bulk-update-prices`, { updates });
  }

  // ==================== STATISTICS ====================

  getProductStatistics(): Observable<ApiResponse<ProductStatistics>> {
    return this.http.get<ApiResponse<ProductStatistics>>(`${this.baseUrl}/statistics`);
  }
}
