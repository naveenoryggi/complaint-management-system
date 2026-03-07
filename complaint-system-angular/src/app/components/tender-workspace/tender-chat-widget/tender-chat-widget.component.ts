import { Component, Input, OnInit, OnChanges, SimpleChanges, inject, signal, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TenderChatService, ChatMessage } from '../../../services/tender-chat.service';

@Component({
  selector: 'app-tender-chat-widget',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './tender-chat-widget.component.html',
  styleUrls: ['./tender-chat-widget.component.css'],
})
export class TenderChatWidgetComponent implements OnInit, OnChanges, AfterViewChecked {
  @Input() tenderId!: string;
  @ViewChild('messageList') messageListEl!: ElementRef;

  private chatService = inject(TenderChatService);

  isOpen = signal(false);
  messages = signal<ChatMessage[]>([]);
  loading = signal(false);
  sending = signal(false);
  messageText = '';
  private shouldScroll = false;

  ngOnInit() {
    if (this.tenderId) {
      this.loadHistory();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['tenderId'] && !changes['tenderId'].firstChange) {
      this.messages.set([]);
      this.loadHistory();
    }
  }

  ngAfterViewChecked() {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  toggle() {
    this.isOpen.set(!this.isOpen());
    if (this.isOpen() && this.messages().length === 0) {
      this.loadHistory();
    }
  }

  loadHistory() {
    if (!this.tenderId) return;
    this.loading.set(true);
    this.chatService.getHistory(this.tenderId).subscribe({
      next: (res) => {
        this.messages.set(res.messages);
        this.loading.set(false);
        this.shouldScroll = true;
      },
      error: () => this.loading.set(false),
    });
  }

  sendMessage() {
    const text = this.messageText.trim();
    if (!text || this.sending()) return;

    this.sending.set(true);
    this.messageText = '';

    // Optimistic: show user message immediately
    const tempUserMsg: ChatMessage = {
      id: 'temp-' + Date.now(),
      tender_id: this.tenderId,
      role: 'user',
      content: text,
      tokens_used: null,
      model_used: null,
      user_name: 'You',
      created_at: new Date().toISOString(),
    };
    this.messages.set([...this.messages(), tempUserMsg]);
    this.shouldScroll = true;

    this.chatService.sendMessage(this.tenderId, text).subscribe({
      next: (res) => {
        // Replace temp message with real ones
        const msgs = this.messages().filter((m) => m.id !== tempUserMsg.id);
        msgs.push(res.user_message, res.assistant_message);
        this.messages.set(msgs);
        this.sending.set(false);
        this.shouldScroll = true;
      },
      error: () => {
        // Remove temp and show error
        const msgs = this.messages().filter((m) => m.id !== tempUserMsg.id);
        msgs.push({
          ...tempUserMsg,
          id: 'err-' + Date.now(),
        });
        msgs.push({
          id: 'err-reply-' + Date.now(),
          tender_id: this.tenderId,
          role: 'assistant',
          content: 'Sorry, I could not process your message. Please try again.',
          tokens_used: null,
          model_used: null,
          user_name: 'AI Assistant',
          created_at: new Date().toISOString(),
        });
        this.messages.set(msgs);
        this.sending.set(false);
        this.shouldScroll = true;
      },
    });
  }

  clearChat() {
    if (!confirm('Clear entire chat history for this tender?')) return;
    this.chatService.clearHistory(this.tenderId).subscribe({
      next: () => this.messages.set([]),
      error: () => {},
    });
  }

  onKeydown(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private scrollToBottom() {
    try {
      const el = this.messageListEl?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    } catch {}
  }

  formatTime(iso: string | null): string {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' });
  }
}
