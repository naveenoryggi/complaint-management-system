import { Component, Input, Output, EventEmitter, forwardRef, OnInit, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface MultiSelectOption {
  id: string;
  label: string;
  code?: string;
  level?: number;
  hasChildren?: boolean;
}

@Component({
  selector: 'app-multi-select-dropdown',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './multi-select-dropdown.component.html',
  styleUrls: ['./multi-select-dropdown.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MultiSelectDropdownComponent),
      multi: true
    }
  ]
})
export class MultiSelectDropdownComponent implements OnInit, ControlValueAccessor {
  @Input() options: MultiSelectOption[] = [];
  @Input() placeholder = 'Select items...';
  @Input() showHierarchy = false;
  @Input() disabled = false;
  @Input() maxDisplayItems = 3;

  @Output() selectionChange = new EventEmitter<string[]>();

  isOpen = false;
  searchTerm = '';
  selectedIds: string[] = [];

  private onChange: (value: string[]) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private elementRef: ElementRef) {}

  ngOnInit(): void {}

  // ControlValueAccessor implementation
  writeValue(value: string[]): void {
    this.selectedIds = value || [];
  }

  registerOnChange(fn: (value: string[]) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  // Close dropdown when clicking outside
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen = false;
    }
  }

  toggleDropdown(): void {
    if (!this.disabled) {
      this.isOpen = !this.isOpen;
      if (this.isOpen) {
        this.onTouched();
      }
    }
  }

  isSelected(id: string): boolean {
    return this.selectedIds.includes(id);
  }

  toggleOption(option: MultiSelectOption, event: Event): void {
    event.stopPropagation();

    const index = this.selectedIds.indexOf(option.id);
    if (index > -1) {
      this.selectedIds = this.selectedIds.filter(id => id !== option.id);
    } else {
      this.selectedIds = [...this.selectedIds, option.id];
    }

    this.onChange(this.selectedIds);
    this.selectionChange.emit(this.selectedIds);
  }

  selectAll(): void {
    this.selectedIds = this.filteredOptions.map(o => o.id);
    this.onChange(this.selectedIds);
    this.selectionChange.emit(this.selectedIds);
  }

  clearAll(): void {
    this.selectedIds = [];
    this.onChange(this.selectedIds);
    this.selectionChange.emit(this.selectedIds);
  }

  get filteredOptions(): MultiSelectOption[] {
    if (!this.searchTerm) {
      return this.options;
    }
    const term = this.searchTerm.toLowerCase();
    return this.options.filter(o =>
      o.label.toLowerCase().includes(term) ||
      (o.code && o.code.toLowerCase().includes(term))
    );
  }

  get selectedOptions(): MultiSelectOption[] {
    return this.options.filter(o => this.selectedIds.includes(o.id));
  }

  get displayText(): string {
    if (this.selectedIds.length === 0) {
      return this.placeholder;
    }

    const selected = this.selectedOptions;
    if (selected.length <= this.maxDisplayItems) {
      return selected.map(o => o.label).join(', ');
    }

    return `${selected.length} items selected`;
  }

  getIndent(level: number | undefined): string {
    if (!this.showHierarchy || !level) return '';
    return '\u00A0\u00A0\u00A0\u00A0'.repeat(level);
  }

  getPrefix(level: number | undefined): string {
    if (!this.showHierarchy || !level || level === 0) return '';
    return '└─ ';
  }

  removeItem(id: string, event: Event): void {
    event.stopPropagation();
    this.selectedIds = this.selectedIds.filter(selectedId => selectedId !== id);
    this.onChange(this.selectedIds);
    this.selectionChange.emit(this.selectedIds);
  }
}
