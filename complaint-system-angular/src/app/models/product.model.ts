// Product Enums
export enum ProductType {
  Physical = 0,
  Digital = 1,
  Service = 2,
  Subscription = 3,
  Bundle = 4,
  SparePart = 5,
  Maintenance = 6
}

export enum ProductStatus {
  Draft = 0,
  Active = 1,
  Inactive = 2,
  Discontinued = 3,
  EndOfLife = 4,
  Archived = 5
}

export enum BillingFrequency {
  OneTime = 0,
  Monthly = 1,
  Quarterly = 2,
  SemiAnnual = 3,
  Annual = 4,
  Biennial = 5,
  Triennial = 6
}

export enum UnitOfMeasure {
  Each = 0,
  Box = 1,
  Pack = 2,
  Kg = 3,
  Gram = 4,
  Liter = 5,
  Meter = 6,
  Hour = 7,
  Day = 8,
  License = 9,
  User = 10,
  Set = 11
}

export enum TaxType {
  Exempt = 0,
  GST = 1,
  IGST = 2,
  VAT = 3,
  Custom = 4
}

export enum PriceListType {
  Standard = 0,
  Partner = 1,
  Customer = 2,
  Volume = 3,
  Promotional = 4,
  Contract = 5
}

// Enum Label Maps
export const ProductTypeLabels: { [key: number]: string } = {
  [ProductType.Physical]: 'Physical',
  [ProductType.Digital]: 'Digital',
  [ProductType.Service]: 'Service',
  [ProductType.Subscription]: 'Subscription',
  [ProductType.Bundle]: 'Bundle',
  [ProductType.SparePart]: 'Spare Part',
  [ProductType.Maintenance]: 'Maintenance'
};

export const ProductStatusLabels: { [key: number]: string } = {
  [ProductStatus.Draft]: 'Draft',
  [ProductStatus.Active]: 'Active',
  [ProductStatus.Inactive]: 'Inactive',
  [ProductStatus.Discontinued]: 'Discontinued',
  [ProductStatus.EndOfLife]: 'End of Life',
  [ProductStatus.Archived]: 'Archived'
};

export const BillingFrequencyLabels: { [key: number]: string } = {
  [BillingFrequency.OneTime]: 'One Time',
  [BillingFrequency.Monthly]: 'Monthly',
  [BillingFrequency.Quarterly]: 'Quarterly',
  [BillingFrequency.SemiAnnual]: 'Semi-Annual',
  [BillingFrequency.Annual]: 'Annual',
  [BillingFrequency.Biennial]: 'Biennial',
  [BillingFrequency.Triennial]: 'Triennial'
};

export const UnitOfMeasureLabels: { [key: number]: string } = {
  [UnitOfMeasure.Each]: 'Each',
  [UnitOfMeasure.Box]: 'Box',
  [UnitOfMeasure.Pack]: 'Pack',
  [UnitOfMeasure.Kg]: 'Kg',
  [UnitOfMeasure.Gram]: 'Gram',
  [UnitOfMeasure.Liter]: 'Liter',
  [UnitOfMeasure.Meter]: 'Meter',
  [UnitOfMeasure.Hour]: 'Hour',
  [UnitOfMeasure.Day]: 'Day',
  [UnitOfMeasure.License]: 'License',
  [UnitOfMeasure.User]: 'User',
  [UnitOfMeasure.Set]: 'Set'
};

export const TaxTypeLabels: { [key: number]: string } = {
  [TaxType.Exempt]: 'Exempt',
  [TaxType.GST]: 'GST',
  [TaxType.IGST]: 'IGST',
  [TaxType.VAT]: 'VAT',
  [TaxType.Custom]: 'Custom'
};

export const PriceListTypeLabels: { [key: number]: string } = {
  [PriceListType.Standard]: 'Standard',
  [PriceListType.Partner]: 'Partner',
  [PriceListType.Customer]: 'Customer',
  [PriceListType.Volume]: 'Volume',
  [PriceListType.Promotional]: 'Promotional',
  [PriceListType.Contract]: 'Contract'
};

// Product Category Interfaces
export interface ProductCategory {
  id: string;
  companyId: string;
  parentCategoryId?: string;
  code: string;
  name: string;
  description?: string;
  level: number;
  path?: string;
  fullPath?: string;
  displayOrder: number;
  icon?: string;
  color?: string;
  imageUrl?: string;
  thumbnailUrl?: string;
  defaultWarrantyMonths?: number;
  defaultResponseTimeHours?: number;
  defaultResolutionTimeHours?: number;
  defaultTaxRate?: number;
  defaultHSNCode?: string;
  defaultSACCode?: string;
  requireSerialNumber: boolean;
  trackInventory: boolean;
  isPublic: boolean;
  allowProducts: boolean;
  allowSubCategories: boolean;
  isActive: boolean;
  slug?: string;
  metaTitle?: string;
  metaDescription?: string;
  keywords?: string;
  externalCategoryId?: string;
  notes?: string;
  customAttributes?: string;
  productCount?: number;
  subCategoryCount?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CategoryTreeNode {
  id: string;
  code: string;
  name: string;
  description?: string;
  icon?: string;
  color?: string;
  level: number;
  displayOrder: number;
  isActive: boolean;
  productCount: number;
  hasChildren: boolean;
  children: CategoryTreeNode[];
}

export interface CreateCategoryRequest {
  parentCategoryId?: string;
  code: string;
  name: string;
  description?: string;
  displayOrder?: number;
  icon?: string;
  color?: string;
  imageUrl?: string;
  defaultWarrantyMonths?: number;
  defaultResponseTimeHours?: number;
  defaultResolutionTimeHours?: number;
  defaultTaxRate?: number;
  defaultHSNCode?: string;
  defaultSACCode?: string;
  requireSerialNumber?: boolean;
  trackInventory?: boolean;
  isPublic?: boolean;
  allowProducts?: boolean;
  allowSubCategories?: boolean;
  isActive?: boolean;
  slug?: string;
  metaTitle?: string;
  metaDescription?: string;
  keywords?: string;
  notes?: string;
}

export interface UpdateCategoryRequest extends CreateCategoryRequest {}

export interface CategoryLookup {
  id: string;
  code: string;
  name: string;
  fullPath: string;
  level: number;
}

// Product Interfaces
export interface Product {
  id: string;
  companyId: string;
  categoryId?: string;
  categoryName?: string;
  subTypeId?: string;
  subTypeName?: string;
  parentProductId?: string;
  productCode: string;
  name: string;
  shortDescription?: string;
  description?: string;
  type: ProductType;
  typeName?: string;
  sku?: string;
  partNumber?: string;
  upc?: string;
  ean?: string;
  isbn?: string;
  hsnCode?: string;
  sacCode?: string;
  unitPrice: number;
  costPrice?: number;
  msrp?: number;
  minimumPrice?: number;
  currency: string;
  taxRate?: number;
  isTaxable: boolean;
  taxType?: TaxType;
  unitOfMeasure: UnitOfMeasure;
  customUnitName?: string;
  minOrderQuantity?: number;
  maxOrderQuantity?: number;
  quantityIncrement?: number;
  quantityPerPack?: number;
  trackInventory: boolean;
  quantityInStock?: number;
  quantityReserved?: number;
  quantityAvailable?: number;
  reorderLevel?: number;
  reorderQuantity?: number;
  leadTimeDays?: number;
  allowBackorder: boolean;
  defaultWarehouse?: string;
  brandId?: string;
  brand?: string;
  brandName?: string;
  manufacturer?: string;
  model?: string;
  version?: string;
  specifications?: string;
  features?: string;
  dimensions?: string;
  weight?: number;
  countryOfOrigin?: string;
  imageUrl?: string;
  thumbnailUrl?: string;
  images?: string;
  videos?: string;
  documents?: string;
  defaultWarrantyMonths?: number;
  extendedWarrantyMonths?: number;
  warrantyTerms?: string;
  supportUrl?: string;
  isServiceable: boolean;
  requiresInstallation: boolean;
  isSubscription: boolean;
  billingFrequency?: BillingFrequency;
  trialPeriodDays?: number;
  setupFee?: number;
  autoRenew: boolean;
  cancellationPolicy?: string;
  status: ProductStatus;
  statusName?: string;
  statusReason?: string;
  launchDate?: string;
  endOfSaleDate?: string;
  endOfLifeDate?: string;
  endOfSupportDate?: string;
  isFeatured: boolean;
  isPublic: boolean;
  isConfigurable: boolean;
  requireSerialNumber: boolean;
  slug?: string;
  metaTitle?: string;
  metaDescription?: string;
  tags?: string;
  keywords?: string;
  externalProductId?: string;
  vendorProductId?: string;
  vendorName?: string;
  customFields?: string;
  notes?: string;
  inStock?: boolean;
  isVariant?: boolean;
  hasVariants?: boolean;
  variantCount?: number;
  priceListCount?: number;
  assetCount?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface ProductSummary {
  id: string;
  categoryId?: string;
  categoryName?: string;
  subTypeId?: string;
  subTypeName?: string;
  productCode: string;
  name: string;
  shortDescription?: string;
  type: ProductType;
  typeName?: string;
  sku?: string;
  unitPrice: number;
  currency: string;
  unitOfMeasure: UnitOfMeasure;
  unitOfMeasureName?: string;
  quantityInStock?: number;
  quantityAvailable?: number;
  inStock?: boolean;
  imageUrl?: string;
  thumbnailUrl?: string;
  brandId?: string;
  brand?: string;
  brandName?: string;
  manufacturer?: string;
  status: ProductStatus;
  statusName?: string;
  isFeatured: boolean;
  isPublic: boolean;
  variantCount?: number;
}

export interface CreateProductRequest {
  categoryId?: string;
  subTypeId?: string;
  parentProductId?: string;
  productCode: string;
  name: string;
  shortDescription?: string;
  description?: string;
  type: ProductType;
  sku?: string;
  partNumber?: string;
  upc?: string;
  ean?: string;
  isbn?: string;
  hsnCode?: string;
  sacCode?: string;
  unitPrice: number;
  costPrice?: number;
  msrp?: number;
  minimumPrice?: number;
  currency?: string;
  taxRate?: number;
  isTaxable?: boolean;
  taxType?: TaxType;
  unitOfMeasure?: UnitOfMeasure;
  customUnitName?: string;
  minOrderQuantity?: number;
  maxOrderQuantity?: number;
  quantityIncrement?: number;
  quantityPerPack?: number;
  trackInventory?: boolean;
  quantityInStock?: number;
  reorderLevel?: number;
  reorderQuantity?: number;
  leadTimeDays?: number;
  allowBackorder?: boolean;
  defaultWarehouse?: string;
  brandId?: string;
  brand?: string;
  manufacturer?: string;
  model?: string;
  version?: string;
  specifications?: string;
  features?: string;
  dimensions?: string;
  weight?: number;
  countryOfOrigin?: string;
  imageUrl?: string;
  thumbnailUrl?: string;
  defaultWarrantyMonths?: number;
  extendedWarrantyMonths?: number;
  warrantyTerms?: string;
  supportUrl?: string;
  isServiceable?: boolean;
  requiresInstallation?: boolean;
  isSubscription?: boolean;
  billingFrequency?: BillingFrequency;
  trialPeriodDays?: number;
  setupFee?: number;
  autoRenew?: boolean;
  cancellationPolicy?: string;
  isFeatured?: boolean;
  isPublic?: boolean;
  isConfigurable?: boolean;
  requireSerialNumber?: boolean;
  slug?: string;
  metaTitle?: string;
  metaDescription?: string;
  tags?: string;
  keywords?: string;
  notes?: string;
}

export interface UpdateProductRequest extends CreateProductRequest {
  status: ProductStatus;
  statusReason?: string;
}

export interface UpdateInventoryRequest {
  quantityInStock: number;
  quantityReserved?: number;
  reorderLevel?: number;
  reorderQuantity?: number;
  defaultWarehouse?: string;
}

export interface ProductLookup {
  id: string;
  productCode: string;
  name: string;
  sku?: string;
  type: ProductType;
  unitPrice: number;
  currency: string;
  inStock: boolean;
}

// Price List Interfaces
export interface ProductPriceList {
  id: string;
  productId: string;
  productCode?: string;
  productName?: string;
  name: string;
  description?: string;
  type: PriceListType;
  typeName?: string;
  partnerId?: string;
  partnerName?: string;
  customerId?: string;
  customerName?: string;
  partnerTier?: string;
  customerSegment?: string;
  unitPrice: number;
  currency: string;
  discountPercent?: number;
  discountAmount?: number;
  markupPercent?: number;
  minQuantity?: number;
  maxQuantity?: number;
  tierPricing?: string;
  validFrom?: string;
  validTo?: string;
  isActive: boolean;
  priority: number;
  minOrderValue?: number;
  promoCode?: string;
  contractId?: string;
  conditions?: string;
  approvalStatus?: string;
  approvedBy?: string;
  approvedAt?: string;
  notes?: string;
  isValid?: boolean;
  isPromotional?: boolean;
  effectiveDiscount?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePriceListRequest {
  productId: string;
  name: string;
  description?: string;
  type: PriceListType;
  partnerId?: string;
  customerId?: string;
  partnerTier?: string;
  customerSegment?: string;
  unitPrice: number;
  currency?: string;
  discountPercent?: number;
  discountAmount?: number;
  markupPercent?: number;
  minQuantity?: number;
  maxQuantity?: number;
  tierPricing?: string;
  validFrom?: string;
  validTo?: string;
  isActive?: boolean;
  priority?: number;
  minOrderValue?: number;
  promoCode?: string;
  notes?: string;
}

export interface UpdatePriceListRequest extends CreatePriceListRequest {}

export interface CalculatePriceRequest {
  productId: string;
  quantity?: number;
  partnerId?: string;
  customerId?: string;
  promoCode?: string;
  contractId?: string;
}

export interface CalculatePriceResponse {
  productId: string;
  quantity: number;
  basePrice: number;
  discountAmount: number;
  taxAmount: number;
  finalPrice: number;
  currency: string;
  priceListApplied?: string;
  breakdown?: PriceBreakdown[];
}

export interface PriceBreakdown {
  description: string;
  amount: number;
  type: string;
}

// Product Statistics
export interface ProductStatistics {
  totalProducts: number;
  activeProducts: number;
  inactiveProducts: number;
  draftProducts: number;
  discontinuedProducts: number;
  lowStockProducts: number;
  outOfStockProducts: number;
  totalCategories: number;
  productsByType: { [key: string]: number };
  productsByCategory: { [key: string]: number };
}
