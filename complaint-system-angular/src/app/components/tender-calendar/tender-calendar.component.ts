import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TenderService } from '../../services/tender.service';

export interface CalendarEvent {
  tenderId: number | string;
  tenderRef: string;
  title: string;
  type: 'deadline' | 'emd_expiry' | 'milestone';
  label: string;
}

export interface CalendarDay {
  date: Date;
  day: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  events: CalendarEvent[];
}

@Component({
  selector: 'app-tender-calendar',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './tender-calendar.component.html',
  styleUrls: ['./tender-calendar.component.css']
})
export class TenderCalendarComponent implements OnInit {
  private tenderService = inject(TenderService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  currentDate = signal<Date>(new Date());
  currentMonth = signal<number>(new Date().getMonth());
  currentYear = signal<number>(new Date().getFullYear());
  calendarDays = signal<CalendarDay[]>([]);
  events = signal<CalendarEvent[]>([]);
  selectedDay = signal<CalendarDay | null>(null);
  loading = signal(false);

  readonly weekDays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  readonly monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  monthYearLabel = computed(() => {
    return `${this.monthNames[this.currentMonth()]} ${this.currentYear()}`;
  });

  ngOnInit(): void {
    this.loadTenders();
  }

  private loadTenders(): void {
    this.loading.set(true);
    this.tenderService.listTenders(1, 500).subscribe({
      next: (response: any) => {
        const tenders = Array.isArray(response) ? response : (response?.items || response?.data || []);
        const allEvents: CalendarEvent[] = [];

        tenders.forEach((tender: any) => {
          // Deadline event
          if (tender.deadline || tender.submission_deadline) {
            const deadlineDate = tender.deadline || tender.submission_deadline;
            allEvents.push({
              tenderId: tender.id,
              tenderRef: tender.reference_number || '',
              title: tender.title || 'Untitled Tender',
              type: 'deadline',
              label: `Deadline: ${tender.reference_number || tender.title}`
            });
          }

          // EMD expiry event
          if (tender.emd_expiry_date) {
            allEvents.push({
              tenderId: tender.id,
              tenderRef: tender.reference_number || '',
              title: tender.title || 'Untitled Tender',
              type: 'emd_expiry',
              label: `EMD Expiry: ${tender.reference_number || tender.title}`
            });
          }

          // Milestone dates (pre-bid meeting, opening dates etc.)
          const milestoneFields = [
            'prebid_meeting_date',
            'technical_bid_opening',
            'financial_bid_opening',
            'start_date'
          ];
          milestoneFields.forEach(field => {
            if (tender[field]) {
              allEvents.push({
                tenderId: tender.id,
                tenderRef: tender.reference_number || '',
                title: tender.title || 'Untitled Tender',
                type: 'milestone',
                label: `${this.fieldLabel(field)}: ${tender.reference_number || tender.title}`
              });
            }
          });
        });

        this.events.set(allEvents);
        this.buildCalendar(tenders);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Error loading tenders for calendar', err);
        this.snackBar.open('Failed to load calendar data', 'Close', { duration: 3000 });
        this.loading.set(false);
        this.buildCalendar([]);
      }
    });
  }

  private fieldLabel(field: string): string {
    const labels: Record<string, string> = {
      prebid_meeting_date: 'Pre-Bid Meeting',
      technical_bid_opening: 'Tech Bid Opening',
      financial_bid_opening: 'Fin Bid Opening',
      start_date: 'Start Date'
    };
    return labels[field] || field;
  }

  buildCalendar(tenders: any[] = []): void {
    const year = this.currentYear();
    const month = this.currentMonth();
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // First day of the month
    const firstDay = new Date(year, month, 1);
    // Last day of the month
    const lastDay = new Date(year, month + 1, 0);

    // Day of week for first day (0=Sun, 1=Mon...) — convert to Mon-first (0=Mon)
    let startDow = firstDay.getDay(); // 0=Sun
    startDow = startDow === 0 ? 6 : startDow - 1; // Mon-first: 0=Mon, 6=Sun

    const days: CalendarDay[] = [];

    // Pad with previous month days
    for (let i = startDow - 1; i >= 0; i--) {
      const d = new Date(year, month, -i);
      d.setHours(0, 0, 0, 0);
      days.push({
        date: d,
        day: d.getDate(),
        isCurrentMonth: false,
        isToday: false,
        events: this.getEventsForDay(d, tenders)
      });
    }

    // Current month days
    for (let d = 1; d <= lastDay.getDate(); d++) {
      const date = new Date(year, month, d);
      date.setHours(0, 0, 0, 0);
      days.push({
        date,
        day: d,
        isCurrentMonth: true,
        isToday: date.getTime() === today.getTime(),
        events: this.getEventsForDay(date, tenders)
      });
    }

    // Pad to fill last week row (ensure total is multiple of 7)
    const remaining = 7 - (days.length % 7);
    if (remaining < 7) {
      for (let i = 1; i <= remaining; i++) {
        const d = new Date(year, month + 1, i);
        d.setHours(0, 0, 0, 0);
        days.push({
          date: d,
          day: d.getDate(),
          isCurrentMonth: false,
          isToday: false,
          events: this.getEventsForDay(d, tenders)
        });
      }
    }

    this.calendarDays.set(days);
  }

  private getEventsForDay(date: Date, tenders: any[]): CalendarEvent[] {
    const events: CalendarEvent[] = [];
    const dateStr = this.toDateStr(date);

    tenders.forEach((tender: any) => {
      const fields: Array<{ field: string; type: CalendarEvent['type']; prefix: string }> = [
        { field: 'deadline', type: 'deadline', prefix: 'Deadline' },
        { field: 'submission_deadline', type: 'deadline', prefix: 'Deadline' },
        { field: 'emd_expiry_date', type: 'emd_expiry', prefix: 'EMD Expiry' },
        { field: 'prebid_meeting_date', type: 'milestone', prefix: 'Pre-Bid Meeting' },
        { field: 'technical_bid_opening', type: 'milestone', prefix: 'Tech Bid Opening' },
        { field: 'financial_bid_opening', type: 'milestone', prefix: 'Fin Bid Opening' },
        { field: 'start_date', type: 'milestone', prefix: 'Start' }
      ];

      fields.forEach(({ field, type, prefix }) => {
        if (tender[field] && this.toDateStr(new Date(tender[field])) === dateStr) {
          events.push({
            tenderId: tender.id,
            tenderRef: tender.reference_number || '',
            title: tender.title || 'Untitled',
            type,
            label: `${prefix}: ${tender.reference_number || tender.title}`
          });
        }
      });
    });

    return events;
  }

  private toDateStr(date: Date): string {
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  }

  prevMonth(): void {
    let m = this.currentMonth() - 1;
    let y = this.currentYear();
    if (m < 0) { m = 11; y--; }
    this.currentMonth.set(m);
    this.currentYear.set(y);
    this.selectedDay.set(null);
    this.buildCalendarFromSignals();
  }

  nextMonth(): void {
    let m = this.currentMonth() + 1;
    let y = this.currentYear();
    if (m > 11) { m = 0; y++; }
    this.currentMonth.set(m);
    this.currentYear.set(y);
    this.selectedDay.set(null);
    this.buildCalendarFromSignals();
  }

  goToToday(): void {
    const today = new Date();
    this.currentMonth.set(today.getMonth());
    this.currentYear.set(today.getFullYear());
    this.selectedDay.set(null);
    this.buildCalendarFromSignals();
  }

  private buildCalendarFromSignals(): void {
    // Rebuild with cached tenders — reload from service for simplicity
    this.loadTenders();
  }

  onDayClick(day: CalendarDay): void {
    if (day.events.length === 0) {
      this.selectedDay.set(null);
      return;
    }
    this.selectedDay.set(day);
  }

  onEventClick(event: CalendarEvent): void {
    this.router.navigate(['/tender-detail', event.tenderId]);
  }

  getEventColor(type: CalendarEvent['type']): string {
    const colors: Record<string, string> = {
      deadline: '#c5221f',
      emd_expiry: '#e37400',
      milestone: '#1a73e8'
    };
    return colors[type] || '#5f6368';
  }

  getEventClass(type: CalendarEvent['type']): string {
    const classes: Record<string, string> = {
      deadline: 'event-deadline',
      emd_expiry: 'event-emd',
      milestone: 'event-milestone'
    };
    return classes[type] || 'event-default';
  }

  clearSelection(): void {
    this.selectedDay.set(null);
  }

  getWeeks(): CalendarDay[][] {
    const days = this.calendarDays();
    const weeks: CalendarDay[][] = [];
    for (let i = 0; i < days.length; i += 7) {
      weeks.push(days.slice(i, i + 7));
    }
    return weeks;
  }
}
