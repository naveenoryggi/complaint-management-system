import { Component, OnInit, OnDestroy, Output, EventEmitter, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, switchMap, catchError, finalize, map } from 'rxjs/operators';
import { Observable, of, throwError, Subject } from 'rxjs';
import { UserService } from '../../../services/user.service';
import { User } from '../../../models/user.model';

export interface UserSearchResult {
  user: User;
  matchType: 'email' | 'name' | 'phone' | 'userId' | 'firstName' | 'lastName';
  highlightedText: {
    email?: string;
    name?: string;
    phone?: string;
    userId?: string;
  };
}

export interface UserSearchConfig {
  placeholder?: string;
  minSearchLength?: number;
  maxResults?: number;
  showUserDetails?: boolean;
  showMatchType?: boolean;
  excludeUserId?: string;
  includeInactive?: boolean;
  searchFields?: string[];
}

@Component({
  selector: 'app-user-search-autocomplete',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-search-autocomplete.component.html',
  styleUrls: ['./user-search-autocomplete.component.scss']
})
export class UserSearchAutocompleteComponent implements OnInit, OnDestroy {
  @Input() config: UserSearchConfig = {};
  @Input() selectedUser?: User | null;
  @Output() userSelected = new EventEmitter<User>();
  @Output() searchCleared = new EventEmitter<void>();

  searchTerm = '';
  searchResults: UserSearchResult[] = [];
  isLoading = false;
  showResults = false;
  errorMessage = '';
  noResults = false;

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  // Default configuration
  private defaultConfig: UserSearchConfig = {
    placeholder: 'Search by name, email, phone, or user ID...',
    minSearchLength: 2,
    maxResults: 10,
    showUserDetails: true,
    showMatchType: true,
    excludeUserId: '',
    includeInactive: false,
    searchFields: ['email', 'fullName', 'phoneNumber', 'employeeCode', 'firstName', 'lastName']
  };

  get mergedConfig(): UserSearchConfig {
    return { ...this.defaultConfig, ...this.config };
  }

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.setupSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.searchSubject.complete();
  }

  /**
   * Setup search with debouncing
   */
  private setupSearch(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        if (term.length < this.mergedConfig.minSearchLength!) {
          return of([]);
        }
        return this.performSearch(term);
      }),
      catchError(error => {
        console.error('Search error:', error);
        this.errorMessage = 'Search failed. Please try again.';
        this.isLoading = false;
        return of([]);
      }),
      finalize(() => {
        this.isLoading = false;
      })
    ).subscribe(results => {
      this.searchResults = results;
      this.noResults = results.length === 0 && this.searchTerm.length >= this.mergedConfig.minSearchLength!;
      this.showResults = results.length > 0 || this.noResults;
    });
  }

  /**
   * Perform user search with multiple criteria
   */
  private performSearch(searchTerm: string): Observable<UserSearchResult[]> {
    this.isLoading = true;
    this.errorMessage = '';

    const searchFields = this.mergedConfig.searchFields!;

    return this.userService.searchUsers(searchTerm, searchFields).pipe(
      map(response => {
        if (!response.isSuccess || !response.data) {
          return [];
        }

        const config = this.mergedConfig;
        let filteredUsers = response.data;

        // Exclude specific user if configured
        if (config.excludeUserId) {
          filteredUsers = response.data.filter(user => user.id !== config.excludeUserId);
        }

        // Filter inactive users if not included
        if (!config.includeInactive) {
          filteredUsers = filteredUsers.filter(user => user.isActive !== false);
        }

        // Limit results
        filteredUsers = filteredUsers.slice(0, config.maxResults!);

        // Create search results with highlighting
        return filteredUsers.map(user => this.createSearchResult(user, searchTerm));
      })
    );
  }

  /**
   * Create search result with highlighted text
   */
  private createSearchResult(user: User, searchTerm: string): UserSearchResult {
    const lowerSearchTerm = searchTerm.toLowerCase();
    const result: UserSearchResult = {
      user,
      matchType: 'name',
      highlightedText: {}
    };

    // Determine match type and create highlights
    if (user.email && user.email.toLowerCase().includes(lowerSearchTerm)) {
      result.matchType = 'email';
      result.highlightedText.email = this.highlightText(user.email, searchTerm);
    } else if (user.fullName && user.fullName.toLowerCase().includes(lowerSearchTerm)) {
      result.matchType = 'name';
      result.highlightedText.name = this.highlightText(user.fullName, searchTerm);
    } else if (user.phoneNumber && user.phoneNumber.toLowerCase().includes(lowerSearchTerm)) {
      result.matchType = 'phone';
      result.highlightedText.phone = this.highlightText(user.phoneNumber, searchTerm);
    } else if (user.employeeCode && user.employeeCode.toLowerCase().includes(lowerSearchTerm)) {
      result.matchType = 'userId';
      result.highlightedText.userId = this.highlightText(user.employeeCode, searchTerm);
    } else if (user.firstName && user.firstName.toLowerCase().includes(lowerSearchTerm)) {
      result.matchType = 'firstName';
      result.highlightedText.name = this.highlightText(user.firstName, searchTerm);
    } else if (user.lastName && user.lastName.toLowerCase().includes(lowerSearchTerm)) {
      result.matchType = 'lastName';
      result.highlightedText.name = this.highlightText(user.lastName, searchTerm);
    }

    return result;
  }

  /**
   * Highlight matching text
   */
  private highlightText(text: string, searchTerm: string): string {
    const regex = new RegExp(`(${this.escapeRegex(searchTerm)})`, 'gi');
    return text.replace(regex, '<mark>$1</mark>');
  }

  /**
   * Escape regex special characters
   */
  private escapeRegex(string: string): string {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  }

  /**
   * Handle search input change
   */
  onSearchChange(): void {
    this.searchSubject.next(this.searchTerm);
  }

  /**
   * Clear search
   */
  clearSearch(): void {
    this.searchTerm = '';
    this.searchResults = [];
    this.showResults = false;
    this.noResults = false;
    this.errorMessage = '';
    this.searchCleared.emit();
  }

  /**
   * Select a user
   */
  selectUser(result: UserSearchResult): void {
    this.selectedUser = result.user;
    this.searchTerm = result.user.fullName || result.user.email || result.user.employeeCode || '';
    this.showResults = false;
    this.userSelected.emit(result.user);
  }

  /**
   * Get display text for user
   */
  getDisplayText(user: User): string {
    if (user.fullName) return user.fullName;
    if (user.email) return user.email;
    if (user.employeeCode) return user.employeeCode;
    return 'Unknown User';
  }

  /**
   * Get user subtitle information
   */
  getUserSubtitle(user: User): string {
    const parts: string[] = [];

    if (user.email && user.fullName) parts.push(user.email);
    if (user.employeeCode) parts.push(`ID: ${user.employeeCode}`);
    if (user.phoneNumber) parts.push(user.phoneNumber);
    if (user.departmentName) parts.push(user.departmentName);
    if (user.employeeTypeName) parts.push(user.employeeTypeName);

    return parts.join(' • ');
  }

  /**
   * Get match type label
   */
  getMatchTypeLabel(matchType: string): string {
    const labels: { [key: string]: string } = {
      email: 'Email',
      name: 'Name',
      phone: 'Phone',
      userId: 'User ID',
      firstName: 'First Name',
      lastName: 'Last Name'
    };
    return labels[matchType] || 'Unknown';
  }

  /**
   * Get match type icon
   */
  getMatchTypeIcon(matchType: string): string {
    const icons: { [key: string]: string } = {
      email: 'bi-envelope',
      name: 'bi-person',
      phone: 'bi-telephone',
      userId: 'bi-badge',
      firstName: 'bi-person',
      lastName: 'bi-person'
    };
    return icons[matchType] || 'bi-search';
  }

  /**
   * Check if user is active
   */
  isUserActive(user: User): boolean {
    return user.isActive !== false;
  }

  /**
   * Handle keyboard navigation
   */
  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      this.clearSearch();
    }
  }

  /**
   * Handle click outside
   */
  onBackdropClick(): void {
    if (this.showResults && !this.isLoading) {
      this.showResults = false;
    }
  }
}