export interface Category {
  id: string;
  name: string;
  code: string;
  description?: string;
  parentCategoryId?: string;
  parentCategoryName?: string;
  defaultPriority: number;
  defaultSlaHours: number;
  isActive: boolean;
  displayOrder: number;
}
