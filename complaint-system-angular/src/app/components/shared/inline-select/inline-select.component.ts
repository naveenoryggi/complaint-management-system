import { Component, Input, Output, EventEmitter, OnInit, forwardRef, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

export interface SelectOption {
  value: any;
  label: string;
  isCustom?: boolean;
}

@Component({
  selector: 'app-inline-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="inline-select" [class.open]="showDropdown" [class.multi]="multiple">
      <!-- Single Select Display -->
      <div class="select-display" *ngIf="!multiple" (click)="toggleDropdown()">
        <span class="selected-text">{{ getSelectedLabel() || placeholder }}</span>
        <div class="select-actions">
          <button class="btn-add" *ngIf="allowAdd && !disabled" (click)="openAddDialog($event)" title="Add new">
            <i class="fa-solid fa-plus"></i>
          </button>
          <i class="fa-solid fa-chevron-down"></i>
        </div>
      </div>

      <!-- Multi Select Display -->
      <div class="multi-select-display" *ngIf="multiple" (click)="toggleDropdown()">
        <div class="selected-tags" *ngIf="selectedValues.length > 0">
          <span class="tag" *ngFor="let val of selectedValues">
            {{ getLabelByValue(val) }}
            <i class="fa-solid fa-times" (click)="removeValue(val, $event)"></i>
          </span>
        </div>
        <span class="placeholder" *ngIf="selectedValues.length === 0">{{ placeholder }}</span>
        <div class="select-actions">
          <button class="btn-add" *ngIf="allowAdd && !disabled" (click)="openAddDialog($event)" title="Add new">
            <i class="fa-solid fa-plus"></i>
          </button>
          <i class="fa-solid fa-chevron-down"></i>
        </div>
      </div>

      <!-- Dropdown -->
      <div class="dropdown" *ngIf="showDropdown">
        <div class="dropdown-search" *ngIf="searchable">
          <input type="text" [(ngModel)]="searchText" placeholder="Search..." (click)="$event.stopPropagation()">
        </div>
        <div class="dropdown-options">
          <div class="option" *ngFor="let option of filteredOptions"
               [class.selected]="isSelected(option.value)"
               [class.custom]="option.isCustom"
               (click)="selectOption(option, $event)">
            <i class="fa-solid fa-check" *ngIf="isSelected(option.value)"></i>
            <span>{{ option.label }}</span>
            <button class="btn-delete" *ngIf="option.isCustom && allowDelete"
                    (click)="deleteOption(option, $event)" title="Delete">
              <i class="fa-solid fa-trash"></i>
            </button>
          </div>
          <div class="no-options" *ngIf="filteredOptions.length === 0">
            No options found
          </div>
        </div>
      </div>

      <!-- Add Dialog -->
      <div class="add-overlay" *ngIf="showAddDialog" (click)="closeAddDialog()">
        <div class="add-dialog" (click)="$event.stopPropagation()">
          <h4>Add New {{ label }}</h4>
          <input type="text" [(ngModel)]="newItemLabel" placeholder="Enter name..."
                 (keyup.enter)="addNewOption()">
          <div class="dialog-actions">
            <button class="btn-cancel" (click)="closeAddDialog()">Cancel</button>
            <button class="btn-save" (click)="addNewOption()" [disabled]="!newItemLabel.trim()">Add</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .inline-select {
      position: relative;
      width: 100%;
    }

    .select-display, .multi-select-display {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 10px 14px;
      border: 1px solid #ddd;
      border-radius: 6px;
      background: white;
      cursor: pointer;
      min-height: 42px;
      transition: border-color 0.2s;
    }

    .select-display:hover, .multi-select-display:hover {
      border-color: #999;
    }

    .inline-select.open .select-display,
    .inline-select.open .multi-select-display {
      border-color: #0057FF;
    }

    .selected-text {
      flex: 1;
      color: #333;
    }

    .selected-text:empty::before,
    .placeholder {
      content: attr(data-placeholder);
      color: #999;
    }

    .placeholder {
      color: #999;
      flex: 1;
    }

    .select-actions {
      display: flex;
      align-items: center;
      gap: 8px;

      i {
        color: #666;
        font-size: 12px;
      }
    }

    .btn-add {
      background: #e8f0ff;
      border: none;
      width: 24px;
      height: 24px;
      border-radius: 4px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.2s;

      i {
        color: #0057FF;
        font-size: 11px;
      }

      &:hover {
        background: #0057FF;
        i { color: white; }
      }
    }

    .multi-select-display {
      flex-wrap: wrap;
      gap: 6px;
    }

    .selected-tags {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
      flex: 1;
    }

    .tag {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      background: #e8f0ff;
      color: #0057FF;
      padding: 4px 10px;
      border-radius: 12px;
      font-size: 13px;

      i {
        cursor: pointer;
        font-size: 10px;
        &:hover { color: #dc3545; }
      }
    }

    .dropdown {
      position: absolute;
      top: 100%;
      left: 0;
      right: 0;
      background: white;
      border: 1px solid #ddd;
      border-radius: 6px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      margin-top: 4px;
      z-index: 100;
      max-height: 250px;
      overflow: hidden;
      display: flex;
      flex-direction: column;
    }

    .dropdown-search {
      padding: 8px;
      border-bottom: 1px solid #eee;

      input {
        width: 100%;
        padding: 8px 12px;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 14px;

        &:focus {
          outline: none;
          border-color: #0057FF;
        }
      }
    }

    .dropdown-options {
      overflow-y: auto;
      max-height: 200px;
    }

    .option {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 10px 14px;
      cursor: pointer;
      transition: background 0.2s;

      &:hover {
        background: #f5f5f5;
      }

      &.selected {
        background: #e8f0ff;
        color: #0057FF;
      }

      &.custom {
        border-left: 3px solid #f0ad4e;
      }

      i.fa-check {
        font-size: 12px;
        width: 14px;
      }

      span {
        flex: 1;
      }

      .btn-delete {
        background: transparent;
        border: none;
        padding: 4px;
        cursor: pointer;
        opacity: 0;
        transition: opacity 0.2s;

        i { color: #dc3545; font-size: 12px; }
      }

      &:hover .btn-delete {
        opacity: 1;
      }
    }

    .no-options {
      padding: 14px;
      text-align: center;
      color: #999;
    }

    .add-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.4);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .add-dialog {
      background: white;
      padding: 20px;
      border-radius: 8px;
      width: 300px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.2);

      h4 {
        margin: 0 0 16px 0;
        color: #333;
      }

      input {
        width: 100%;
        padding: 10px 14px;
        border: 1px solid #ddd;
        border-radius: 6px;
        font-size: 14px;
        margin-bottom: 16px;

        &:focus {
          outline: none;
          border-color: #0057FF;
        }
      }

      .dialog-actions {
        display: flex;
        gap: 10px;
        justify-content: flex-end;
      }

      .btn-cancel {
        background: #f5f5f5;
        border: 1px solid #ddd;
        padding: 8px 16px;
        border-radius: 6px;
        cursor: pointer;

        &:hover { background: #e8e8e8; }
      }

      .btn-save {
        background: #0057FF;
        color: white;
        border: none;
        padding: 8px 16px;
        border-radius: 6px;
        cursor: pointer;

        &:hover { background: #0046cc; }
        &:disabled { background: #ccc; cursor: not-allowed; }
      }
    }
  `],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InlineSelectComponent),
      multi: true
    }
  ]
})
export class InlineSelectComponent implements OnInit, ControlValueAccessor {
  @Input() options: SelectOption[] = [];
  @Input() placeholder = 'Select...';
  @Input() label = 'Item';
  @Input() multiple = false;
  @Input() searchable = false;
  @Input() allowAdd = false;
  @Input() allowDelete = false;
  @Input() disabled = false;

  @Output() optionAdded = new EventEmitter<string>();
  @Output() optionDeleted = new EventEmitter<SelectOption>();

  showDropdown = false;
  showAddDialog = false;
  searchText = '';
  newItemLabel = '';

  selectedValue: any = null;
  selectedValues: any[] = [];

  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private elementRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.showDropdown = false;
    }
  }

  ngOnInit(): void {}

  get filteredOptions(): SelectOption[] {
    if (!this.searchText) return this.options;
    const search = this.searchText.toLowerCase();
    return this.options.filter(o => o.label.toLowerCase().includes(search));
  }

  getSelectedLabel(): string {
    if (this.selectedValue === null || this.selectedValue === undefined) return '';
    const option = this.options.find(o => o.value === this.selectedValue);
    return option?.label || '';
  }

  getLabelByValue(value: any): string {
    const option = this.options.find(o => o.value === value);
    return option?.label || value;
  }

  isSelected(value: any): boolean {
    if (this.multiple) {
      return this.selectedValues.includes(value);
    }
    return this.selectedValue === value;
  }

  toggleDropdown(): void {
    if (this.disabled) return;
    this.showDropdown = !this.showDropdown;
  }

  selectOption(option: SelectOption, event: Event): void {
    event.stopPropagation();

    if (this.multiple) {
      const index = this.selectedValues.indexOf(option.value);
      if (index > -1) {
        this.selectedValues.splice(index, 1);
      } else {
        this.selectedValues.push(option.value);
      }
      this.onChange([...this.selectedValues]);
    } else {
      this.selectedValue = option.value;
      this.onChange(this.selectedValue);
      this.showDropdown = false;
    }
  }

  removeValue(value: any, event: Event): void {
    event.stopPropagation();
    const index = this.selectedValues.indexOf(value);
    if (index > -1) {
      this.selectedValues.splice(index, 1);
      this.onChange([...this.selectedValues]);
    }
  }

  openAddDialog(event: Event): void {
    event.stopPropagation();
    this.showAddDialog = true;
    this.newItemLabel = '';
  }

  closeAddDialog(): void {
    this.showAddDialog = false;
    this.newItemLabel = '';
  }

  addNewOption(): void {
    if (!this.newItemLabel.trim()) return;
    this.optionAdded.emit(this.newItemLabel.trim());
    this.closeAddDialog();
  }

  deleteOption(option: SelectOption, event: Event): void {
    event.stopPropagation();
    if (confirm(`Delete "${option.label}"?`)) {
      this.optionDeleted.emit(option);
    }
  }

  // ControlValueAccessor implementation
  writeValue(value: any): void {
    if (this.multiple) {
      this.selectedValues = Array.isArray(value) ? value : [];
    } else {
      this.selectedValue = value;
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
