// Stock Management Enums
export enum StockItemStatus {
  Active = 0,
  Inactive = 1,
  OnHold = 2,
  Discontinued = 3,
  Reserved = 4
}

export enum StockCondition {
  New = 0,
  Good = 1,
  Fair = 2,
  Damaged = 3,
  Refurbished = 4,
  ForRepair = 5
}

export enum CostingMethod {
  FIFO = 0,
  LIFO = 1,
  WeightedAverage = 2,
  SpecificIdentification = 3,
  StandardCost = 4
}

export enum StockMovementType {
  // Incoming
  Receipt = 0,
  Return = 1,
  TransferIn = 2,
  Production = 3,
  Adjustment_In = 4,
  // Outgoing
  Issue = 10,
  Sale = 11,
  TransferOut = 12,
  Consumption = 13,
  Adjustment_Out = 14,
  Scrap = 15,
  Loss = 16,
  // Status changes
  CategoryChange = 20,
  Reservation = 21,
  Unreservation = 22,
  // Inventory
  StockCount = 30,
  Revaluation = 31
}

export enum StockMovementStatus {
  Draft = 0,
  Pending = 1,
  Approved = 2,
  Completed = 3,
  Cancelled = 4,
  Reversed = 5
}

export enum StockReferenceType {
  PurchaseOrder = 0,
  SalesOrder = 1,
  TransferOrder = 2,
  ServiceRequest = 3,
  Complaint = 4,
  Project = 5,
  Contract = 6,
  Warranty = 7,
  Manual = 8
}

export enum StockLocationType {
  Warehouse = 0,
  Store = 1,
  Zone = 2,
  Aisle = 3,
  Rack = 4,
  Shelf = 5,
  Bin = 6,
  Virtual = 7,
  CustomerSite = 8,
  VendorSite = 9,
  InTransit = 10
}

// Enum Label Maps
export const StockItemStatusLabels: { [key: number]: string } = {
  [StockItemStatus.Active]: 'Active',
  [StockItemStatus.Inactive]: 'Inactive',
  [StockItemStatus.OnHold]: 'On Hold',
  [StockItemStatus.Discontinued]: 'Discontinued',
  [StockItemStatus.Reserved]: 'Reserved'
};

export const StockConditionLabels: { [key: number]: string } = {
  [StockCondition.New]: 'New',
  [StockCondition.Good]: 'Good',
  [StockCondition.Fair]: 'Fair',
  [StockCondition.Damaged]: 'Damaged',
  [StockCondition.Refurbished]: 'Refurbished',
  [StockCondition.ForRepair]: 'For Repair'
};

export const CostingMethodLabels: { [key: number]: string } = {
  [CostingMethod.FIFO]: 'FIFO',
  [CostingMethod.LIFO]: 'LIFO',
  [CostingMethod.WeightedAverage]: 'Weighted Average',
  [CostingMethod.SpecificIdentification]: 'Specific Identification',
  [CostingMethod.StandardCost]: 'Standard Cost'
};

export const StockMovementTypeLabels: { [key: number]: string } = {
  [StockMovementType.Receipt]: 'Receipt (Purchase)',
  [StockMovementType.Return]: 'Customer Return',
  [StockMovementType.TransferIn]: 'Transfer In',
  [StockMovementType.Production]: 'Production Output',
  [StockMovementType.Adjustment_In]: 'Adjustment (+)',
  [StockMovementType.Issue]: 'Issue to Customer/Project',
  [StockMovementType.Sale]: 'Sale',
  [StockMovementType.TransferOut]: 'Transfer Out',
  [StockMovementType.Consumption]: 'Consumption',
  [StockMovementType.Adjustment_Out]: 'Adjustment (-)',
  [StockMovementType.Scrap]: 'Scrap/Dispose',
  [StockMovementType.Loss]: 'Loss/Theft',
  [StockMovementType.CategoryChange]: 'Category Change',
  [StockMovementType.Reservation]: 'Reserve Stock',
  [StockMovementType.Unreservation]: 'Release Reservation',
  [StockMovementType.StockCount]: 'Stock Count',
  [StockMovementType.Revaluation]: 'Cost Revaluation'
};

export const StockMovementStatusLabels: { [key: number]: string } = {
  [StockMovementStatus.Draft]: 'Draft',
  [StockMovementStatus.Pending]: 'Pending',
  [StockMovementStatus.Approved]: 'Approved',
  [StockMovementStatus.Completed]: 'Completed',
  [StockMovementStatus.Cancelled]: 'Cancelled',
  [StockMovementStatus.Reversed]: 'Reversed'
};

export const StockLocationTypeLabels: { [key: number]: string } = {
  [StockLocationType.Warehouse]: 'Warehouse',
  [StockLocationType.Store]: 'Store',
  [StockLocationType.Zone]: 'Zone',
  [StockLocationType.Aisle]: 'Aisle',
  [StockLocationType.Rack]: 'Rack',
  [StockLocationType.Shelf]: 'Shelf',
  [StockLocationType.Bin]: 'Bin',
  [StockLocationType.Virtual]: 'Virtual',
  [StockLocationType.CustomerSite]: 'Customer Site',
  [StockLocationType.VendorSite]: 'Vendor Site',
  [StockLocationType.InTransit]: 'In Transit'
};

// Stock Location Interfaces
export interface StockLocation {
  id: string;
  companyId: string;
  parentLocationId?: string;
  parentLocationName?: string;
  code: string;
  name: string;
  description?: string;
  type: StockLocationType;
  typeName?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  fullAddress?: string;
  contactPerson?: string;
  contactPhone?: string;
  contactEmail?: string;
  latitude?: number;
  longitude?: number;
  capacity?: number;
  currentUtilization?: number;
  utilizationPercent?: number;
  isActive: boolean;
  isDefault: boolean;
  path?: string;
  level: number;
  childCount?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface StockLocationLookup {
  id: string;
  code: string;
  name: string;
  type: StockLocationType;
  fullPath?: string;
}

export interface CreateStockLocationRequest {
  parentLocationId?: string;
  code: string;
  name: string;
  description?: string;
  type: StockLocationType;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  contactPerson?: string;
  contactPhone?: string;
  contactEmail?: string;
  capacity?: number;
  isActive?: boolean;
}

export interface UpdateStockLocationRequest extends CreateStockLocationRequest {}

// Stock Item Interfaces
export interface StockItem {
  id: string;
  companyId: string;
  productId: string;
  productCode?: string;
  productName?: string;
  sku?: string;
  stockCategoryId: string;
  stockCategoryName?: string;
  stockCategoryCode?: string;
  locationId?: string;
  locationName?: string;
  locationCode?: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
  quantityInTransit: number;
  quantityOnOrder: number;
  minimumQuantity?: number;
  maximumQuantity?: number;
  reorderQuantity?: number;
  safetyStock?: number;
  unitOfMeasure: string;
  unitCost?: number;
  totalValue: number;
  currency: string;
  costingMethod: CostingMethod;
  lastPurchasePrice?: number;
  lastPurchaseDate?: string;
  status: StockItemStatus;
  condition: StockCondition;
  requiresAttention: boolean;
  attentionReason?: string;
  lastCountDate?: string;
  lastCountQuantity?: number;
  lastCountVariance?: number;
  lastMovementDate?: string;
  daysSinceLastMovement?: number;
  expiryDate?: string;
  batchNumber?: string;
  customerId?: string;
  customerName?: string;
  projectId?: string;
  projectName?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface StockItemLookup {
  id: string;
  productCode?: string;
  productName?: string;
  sku?: string;
  locationName?: string;
  stockCategoryName?: string;
  quantityAvailable: number;
}

export interface CreateStockItemRequest {
  productId: string;
  stockCategoryId: string;
  locationId?: string;
  quantityOnHand: number;
  quantityReserved?: number;
  minimumQuantity?: number;
  maximumQuantity?: number;
  reorderQuantity?: number;
  safetyStock?: number;
  unitOfMeasure?: string;
  unitCost?: number;
  currency?: string;
  costingMethod?: CostingMethod;
  status?: StockItemStatus;
  condition?: StockCondition;
  expiryDate?: string;
  batchNumber?: string;
  customerId?: string;
  projectId?: string;
  notes?: string;
}

export interface UpdateStockItemRequest {
  minimumQuantity?: number;
  maximumQuantity?: number;
  reorderQuantity?: number;
  safetyStock?: number;
  unitOfMeasure?: string;
  unitCost?: number;
  currency?: string;
  costingMethod?: CostingMethod;
  status?: StockItemStatus;
  condition?: StockCondition;
  expiryDate?: string;
  batchNumber?: string;
  customerId?: string;
  projectId?: string;
  notes?: string;
}

export interface AdjustStockRequest {
  stockItemId: string;
  adjustmentQuantity: number;
  reason: string;
  notes?: string;
}

export interface TransferStockRequest {
  stockItemId: string;
  toLocationId: string;
  toStockCategoryId?: string;
  quantity: number;
  reason?: string;
  notes?: string;
}

// Stock Movement Interfaces
export interface StockMovement {
  id: string;
  companyId: string;
  stockItemId?: string;
  assetId?: string;
  assetTag?: string;
  productId: string;
  productCode?: string;
  productName?: string;
  movementType: StockMovementType;
  movementNumber: string;
  fromLocationId?: string;
  fromLocationName?: string;
  toLocationId?: string;
  toLocationName?: string;
  fromStockCategoryId?: string;
  fromStockCategoryName?: string;
  toStockCategoryId?: string;
  toStockCategoryName?: string;
  quantity: number;
  unitOfMeasure: string;
  quantityBefore?: number;
  quantityAfter?: number;
  referenceType?: StockReferenceType;
  referenceNumber?: string;
  referenceId?: string;
  complaintId?: string;
  complaintNumber?: string;
  customerId?: string;
  customerName?: string;
  projectId?: string;
  projectName?: string;
  unitCost?: number;
  totalValue?: number;
  currency: string;
  movementDate: string;
  reason?: string;
  status: StockMovementStatus;
  serialNumber?: string;
  batchNumber?: string;
  expiryDate?: string;
  performedByUserId?: string;
  performedByUserName?: string;
  approvedByUserId?: string;
  approvedByUserName?: string;
  approvalDate?: string;
  notes?: string;
  createdAt: string;
}

export interface CreateStockMovementRequest {
  stockItemId?: string;
  assetId?: string;
  productId: string;
  movementType: StockMovementType;
  fromLocationId?: string;
  toLocationId?: string;
  fromStockCategoryId?: string;
  toStockCategoryId?: string;
  quantity: number;
  unitOfMeasure?: string;
  referenceType?: StockReferenceType;
  referenceNumber?: string;
  referenceId?: string;
  complaintId?: string;
  customerId?: string;
  projectId?: string;
  unitCost?: number;
  currency?: string;
  movementDate?: string;
  reason?: string;
  serialNumber?: string;
  batchNumber?: string;
  expiryDate?: string;
  notes?: string;
}

export interface StockMovementFilter {
  fromDate?: string;
  toDate?: string;
  movementType?: StockMovementType;
  status?: StockMovementStatus;
  productId?: string;
  locationId?: string;
  stockCategoryId?: string;
  customerId?: string;
  searchTerm?: string;
}

export interface MovementTypeLookup {
  value: number;
  name: string;
  label: string;
  category: string;
}

// Statistics
export interface StockStatistics {
  totalStockItems: number;
  totalValue: number;
  lowStockCount: number;
  expiringCount: number;
  noMovementCount: number;
  byCategory: { [key: string]: number };
  byLocation: { [key: string]: number };
}
