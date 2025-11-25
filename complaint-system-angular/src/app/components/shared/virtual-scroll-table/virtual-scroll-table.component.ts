import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { FormsModule } from '@angular/forms';

export interface TableColumn {
  key: string;
  label: string;
  width?: string;
  sortable?: boolean;
  format?: (value: any, item: any) => string;
  class?: (value: any, item: any) => string;
}

export interface SortEvent {
  column: string;
  direction: 'asc' | 'desc';
}

@Component({
  selector: 'app-virtual-scroll-table',
  standalone: true,
  imports: [CommonModule, ScrollingModule, FormsModule],
  templateUrl: './virtual-scroll-table.component.html',
  styleUrls: ['./virtual-scroll-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VirtualScrollTableComponent<T> implements OnInit, OnChanges {
  @Input() data: T[] = [];
  @Input() columns: TableColumn[] = [];
  @Input() itemHeight = 50;
  @Input() loading = false;
  @Input() emptyMessage = 'No data available';
  @Input() trackBy: (item: T) => string = (item: any) => item.id || JSON.stringify(item);
  @Input() sortable = false;
  @Input() selectable = false;
  @Input() multiSelect = false;
  @Input() selectedItems: T[] = [];

  @Output() sort = new EventEmitter<SortEvent>();
  @Output() rowClick = new EventEmitter<T>();
  @Output() selectionChange = new EventEmitter<T[]>();

  filteredData: T[] = [];
  currentSort: SortEvent | null = null;
  searchTerm = '';
  buffer_size = 10;
  randomId = Math.random().toString(36).substr(2, 9);

  ngOnInit() {
    this.updateFilteredData();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['data']) {
      this.updateFilteredData();
    }
    if (changes['selectedItems']) {
      // Ensure selected items are properly tracked
      this.selectedItems = [...this.selectedItems];
    }
  }

  updateFilteredData() {
    this.filteredData = this.data;
  }

  onSearch(event: any) {
    const term = event.target.value.toLowerCase();
    this.searchTerm = term;

    if (!term) {
      this.filteredData = this.data;
      return;
    }

    this.filteredData = this.data.filter(item => {
      return this.columns.some(column => {
        const value = this.getNestedValue(item, column.key);
        return value && value.toString().toLowerCase().includes(term);
      });
    });
  }

  getNestedValue(item: any, path: string): any {
    return path.split('.').reduce((current, key) => current && current[key], item);
  }

  sortData(column: string) {
    if (!this.sortable) return;

    const direction = this.currentSort?.column === column && this.currentSort.direction === 'asc' ? 'desc' : 'asc';

    this.currentSort = { column, direction };

    this.filteredData = [...this.filteredData].sort((a, b) => {
      const valueA = this.getNestedValue(a, column);
      const valueB = this.getNestedValue(b, column);

      let comparison = 0;
      if (valueA < valueB) comparison = -1;
      if (valueA > valueB) comparison = 1;

      return direction === 'desc' ? -comparison : comparison;
    });

    this.sort.emit(this.currentSort);
  }

  onRowClick(item: T) {
    if (this.selectable) {
      this.toggleSelection(item);
    }
    this.rowClick.emit(item);
  }

  toggleSelection(item: T) {
    const index = this.selectedItems.findIndex(selected =>
      this.trackBy(selected) === this.trackBy(item)
    );

    if (this.multiSelect) {
      if (index >= 0) {
        this.selectedItems.splice(index, 1);
      } else {
        this.selectedItems.push(item);
      }
    } else {
      this.selectedItems = index >= 0 ? [] : [item];
    }

    this.selectionChange.emit([...this.selectedItems]);
  }

  isSelected(item: T): boolean {
    return this.selectedItems.some(selected =>
      this.trackBy(selected) === this.trackBy(item)
    );
  }

  getDisplayValue(item: T, column: TableColumn): string {
    const value = this.getNestedValue(item, column.key);
    return column.format ? column.format(value, item) : (value || '').toString();
  }

  getCellClass(item: T, column: TableColumn): string {
    const baseClass = column.class ? column.class(this.getNestedValue(item, column.key), item) : '';
    return baseClass;
  }

  getHeaderClass(column: TableColumn): string {
    let classes = ['sortable-header'];

    if (this.sortable && column.sortable !== false) {
      classes.push('clickable');
    }

    if (this.currentSort?.column === column.key) {
      classes.push(`sort-${this.currentSort.direction}`);
    }

    return classes.join(' ');
  }

  // CRITICAL FIX: Arrow function to preserve 'this' context when passed to CDK virtual scroll
  trackByItem = (index: number, item: T): string => {
    return this.trackBy(item);
  };

  clearSelection() {
    this.selectedItems = [];
    this.selectionChange.emit([]);
  }

  selectAll() {
    if (this.multiSelect) {
      this.selectedItems = [...this.filteredData];
      this.selectionChange.emit([...this.selectedItems]);
    }
  }

  getSelectedCount(): number {
    return this.selectedItems.length;
  }

  getTotalCount(): number {
    return this.filteredData.length;
  }

  getVisibleCount(): number {
    return this.filteredData.length;
  }
}