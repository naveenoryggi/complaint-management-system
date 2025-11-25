import { Component, Input, OnInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { PasswordService, PasswordStrengthResult } from '../../../services/password.service';

@Component({
  selector: 'app-password-strength-meter',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './password-strength-meter.component.html',
  styleUrls: ['./password-strength-meter.component.scss']
})
export class PasswordStrengthMeterComponent implements OnInit, OnDestroy, OnChanges {
  @Input() password: string = '';
  @Input() showLabel: boolean = true;

  strength: PasswordStrengthResult | null = null;
  isCalculating: boolean = false;

  private passwordChange$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(private passwordService: PasswordService) {}

  ngOnInit(): void {
    // Debounce password changes to avoid excessive API calls
    this.passwordChange$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(password => {
        if (password && password.length > 0) {
          this.calculateStrength(password);
        } else {
          this.strength = null;
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['password']) {
      this.passwordChange$.next(this.password);
    }
  }

  private calculateStrength(password: string): void {
    this.isCalculating = true;

    this.passwordService.checkPasswordStrength(password).subscribe({
      next: (result) => {
        this.strength = result;
        this.isCalculating = false;
      },
      error: (error) => {
        console.error('Error calculating password strength:', error);
        this.isCalculating = false;
        // Fallback to local calculation
        this.strength = {
          score: 0,
          category: 'Unknown',
          colorCode: '#6c757d'
        };
      }
    });
  }

  getProgressWidth(): string {
    return this.strength ? `${this.strength.score}%` : '0%';
  }

  getProgressColor(): string {
    return this.strength?.colorCode || '#6c757d';
  }
}
