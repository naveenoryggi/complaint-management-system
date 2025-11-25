import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

export interface UserSearchResult {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  jobTitle?: string;
}

@Component({
  selector: 'app-user-autocomplete',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-autocomplete.component.html',
  styleUrls: ['./user-autocomplete.component.scss']
})
export class UserAutocompleteComponent implements OnInit, OnDestroy {
  @Input() label: string = 'Select User';
  @Input() placeholder: string = 'Search by name, email, or employee code...';
  @Input() selectedUserId?: string;
  @Input() selectedUserName?: string;
  @Output() userSelected = new EventEmitter<UserSearchResult | null>();

  searchTerm: string = '';
  searchResults: UserSearchResult[] = [];
  isLoading: boolean = false;
  showDropdown: boolean = false;
  selectedUser: UserSearchResult | null = null;

  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    // Setup debounced search
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300), // Wait 300ms after user stops typing
      distinctUntilChanged(), // Only search if the term changed
      switchMap(term => this.searchUsers(term))
    ).subscribe({
      next: (results) => {
        this.searchResults = results;
        this.isLoading = false;
        this.showDropdown = true;
      },
      error: (error) => {
        console.error('Error searching users:', error);
        this.searchResults = [];
        this.isLoading = false;
        this.showDropdown = false;
      }
    });

    // If a user is already selected, set the display name
    if (this.selectedUserId && this.selectedUserName) {
      this.selectedUser = {
        id: this.selectedUserId,
        fullName: this.selectedUserName,
        employeeCode: '',
        firstName: '',
        lastName: '',
        email: ''
      };
    }
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  onSearchInput(event: Event): void {
    const term = (event.target as HTMLInputElement).value;
    this.searchTerm = term;

    if (term.length >= 2) {
      this.isLoading = true;
      this.searchSubject.next(term);
    } else {
      this.searchResults = [];
      this.showDropdown = false;
      this.isLoading = false;
    }
  }

  private searchUsers(searchTerm: string): Promise<UserSearchResult[]> {
    if (!searchTerm || searchTerm.length < 2) {
      return Promise.resolve([]);
    }

    const token = localStorage.getItem('auth_token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    return this.http.get<any>(`${environment.apiUrl}/users/search?searchTerm=${encodeURIComponent(searchTerm)}&limit=20`, { headers })
      .toPromise()
      .then(response => response?.data || [])
      .catch(error => {
        console.error('Search error:', error);
        return [];
      });
  }

  selectUser(user: UserSearchResult): void {
    this.selectedUser = user;
    this.searchTerm = user.fullName;
    this.showDropdown = false;
    this.userSelected.emit(user);
  }

  clearSelection(): void {
    this.selectedUser = null;
    this.searchTerm = '';
    this.searchResults = [];
    this.showDropdown = false;
    this.userSelected.emit(null);
  }

  onInputFocus(): void {
    if (this.searchResults.length > 0) {
      this.showDropdown = true;
    }
  }

  onInputBlur(): void {
    // Delay hiding dropdown to allow click on results
    setTimeout(() => {
      this.showDropdown = false;
    }, 200);
  }
}
