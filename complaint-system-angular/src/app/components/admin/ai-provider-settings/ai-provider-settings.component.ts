import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { Router } from '@angular/router';
import {
  AIProviderService,
  AIProvider,
  AIProviderCreate,
  AIProviderUpdate,
  TestConnectionResult,
  FeatureMapping,
} from '../../../services/ai-provider.service';

interface ProviderPreset {
  provider_name: string;
  display_label: string;
  icon: string;
  default_model: string;
  model_suggestions: string[];
  has_endpoint: boolean;
}

@Component({
  selector: 'app-ai-provider-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatSlideToggleModule,
    MatChipsModule,
    MatDividerModule,
  ],
  templateUrl: './ai-provider-settings.component.html',
  styleUrls: ['./ai-provider-settings.component.css'],
})
export class AIProviderSettingsComponent implements OnInit {
  private providerService = inject(AIProviderService);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  providers = signal<AIProvider[]>([]);
  loading = signal(false);
  showForm = signal(false);
  editingId = signal<string | null>(null);
  testingId = signal<string | null>(null);
  testResult = signal<TestConnectionResult | null>(null);

  // Form data
  formData: AIProviderCreate & { id?: string } = this.getEmptyForm();

  presets: ProviderPreset[] = [
    {
      provider_name: 'anthropic',
      display_label: 'Anthropic (Claude)',
      icon: 'smart_toy',
      default_model: 'claude-sonnet-4-5-20250514',
      model_suggestions: ['claude-sonnet-4-5-20250514', 'claude-opus-4-20250514', 'claude-haiku-4-5-20251001'],
      has_endpoint: false,
    },
    {
      provider_name: 'openai',
      display_label: 'OpenAI (GPT)',
      icon: 'psychology',
      default_model: 'gpt-4o',
      model_suggestions: ['gpt-4o', 'gpt-4o-mini', 'gpt-4-turbo', 'o1-mini'],
      has_endpoint: false,
    },
    {
      provider_name: 'google',
      display_label: 'Google (Gemini)',
      icon: 'auto_awesome',
      default_model: 'gemini-2.0-flash',
      model_suggestions: ['gemini-2.0-flash', 'gemini-2.0-pro', 'gemini-1.5-flash'],
      has_endpoint: false,
    },
    {
      provider_name: 'azure_openai',
      display_label: 'Azure OpenAI',
      icon: 'cloud',
      default_model: 'gpt-4o',
      model_suggestions: ['gpt-4o', 'gpt-4', 'gpt-35-turbo'],
      has_endpoint: true,
    },
  ];

  featureKeys: { key: string; label: string }[] = [
    { key: 'extraction', label: 'AI Extraction' },
    { key: 'declarations', label: 'Declarations' },
    { key: 'prebid', label: 'Pre-Bid Queries' },
    { key: 'document_generation', label: 'Document Generation' },
    { key: 'alignment', label: 'Alignment Verification' },
  ];

  ngOnInit() {
    this.loadProviders();
  }

  loadProviders() {
    this.loading.set(true);
    this.providerService.listProviders().subscribe({
      next: (res) => {
        this.providers.set(res.providers);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.snackBar.open('Failed to load providers', 'Close', { duration: 3000 });
      }
    });
  }

  getEmptyForm(): AIProviderCreate & { id?: string } {
    return {
      provider_name: 'anthropic',
      display_name: '',
      api_key: '',
      endpoint_url: '',
      deployment_name: '',
      api_version: '',
      default_model: 'claude-sonnet-4-5-20250514',
      feature_mapping: {},
      is_active: true,
      is_default: false,
    };
  }

  onAddFromPreset(preset: ProviderPreset) {
    this.editingId.set(null);
    this.formData = {
      provider_name: preset.provider_name,
      display_name: preset.display_label,
      api_key: '',
      endpoint_url: '',
      deployment_name: '',
      api_version: preset.has_endpoint ? '2024-02-01' : '',
      default_model: preset.default_model,
      feature_mapping: {},
      is_active: true,
      is_default: this.providers().length === 0,
    };
    this.showForm.set(true);
  }

  onEdit(provider: AIProvider) {
    this.editingId.set(provider.id);
    this.formData = {
      id: provider.id,
      provider_name: provider.provider_name,
      display_name: provider.display_name,
      api_key: '', // Empty = don't change
      endpoint_url: provider.endpoint_url || '',
      deployment_name: provider.deployment_name || '',
      api_version: provider.api_version || '',
      default_model: provider.default_model,
      feature_mapping: { ...provider.feature_mapping },
      is_active: provider.is_active,
      is_default: provider.is_default,
    };
    this.showForm.set(true);
  }

  onCancelForm() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.formData = this.getEmptyForm();
  }

  onSave() {
    if (!this.formData.display_name || (!this.editingId() && !this.formData.api_key)) {
      this.snackBar.open('Display name and API key are required', 'Close', { duration: 3000 });
      return;
    }

    if (this.editingId()) {
      const updateData: AIProviderUpdate = {};
      if (this.formData.display_name) updateData.display_name = this.formData.display_name;
      if (this.formData.api_key) updateData.api_key = this.formData.api_key;
      if (this.formData.endpoint_url !== undefined) updateData.endpoint_url = this.formData.endpoint_url;
      if (this.formData.deployment_name !== undefined) updateData.deployment_name = this.formData.deployment_name;
      if (this.formData.api_version !== undefined) updateData.api_version = this.formData.api_version;
      if (this.formData.default_model) updateData.default_model = this.formData.default_model;
      updateData.feature_mapping = this.formData.feature_mapping;
      updateData.is_active = this.formData.is_active;
      updateData.is_default = this.formData.is_default;

      this.providerService.updateProvider(this.editingId()!, updateData).subscribe({
        next: () => {
          this.snackBar.open('Provider updated', 'Close', { duration: 3000 });
          this.onCancelForm();
          this.loadProviders();
        },
        error: () => this.snackBar.open('Failed to update', 'Close', { duration: 3000 }),
      });
    } else {
      this.providerService.createProvider(this.formData).subscribe({
        next: () => {
          this.snackBar.open('Provider created', 'Close', { duration: 3000 });
          this.onCancelForm();
          this.loadProviders();
        },
        error: () => this.snackBar.open('Failed to create', 'Close', { duration: 3000 }),
      });
    }
  }

  onDelete(provider: AIProvider) {
    if (!confirm(`Delete ${provider.display_name}?`)) return;
    this.providerService.deleteProvider(provider.id).subscribe({
      next: () => {
        this.snackBar.open('Provider deleted', 'Close', { duration: 3000 });
        this.loadProviders();
      },
      error: () => this.snackBar.open('Failed to delete', 'Close', { duration: 3000 }),
    });
  }

  onTestConnection(provider: AIProvider) {
    this.testingId.set(provider.id);
    this.testResult.set(null);
    this.providerService.testConnection(provider.id).subscribe({
      next: (result) => {
        this.testResult.set(result);
        this.testingId.set(null);
        if (result.success) {
          this.snackBar.open(`Connection OK (${result.latency_ms}ms)`, 'Close', { duration: 3000 });
        } else {
          this.snackBar.open(`Connection failed: ${result.error}`, 'Close', { duration: 5000 });
        }
        this.loadProviders();
      },
      error: () => {
        this.testingId.set(null);
        this.snackBar.open('Test failed', 'Close', { duration: 3000 });
      }
    });
  }

  onSetDefault(provider: AIProvider) {
    this.providerService.setDefault(provider.id).subscribe({
      next: () => {
        this.snackBar.open(`${provider.display_name} set as default`, 'Close', { duration: 3000 });
        this.loadProviders();
      },
      error: () => this.snackBar.open('Failed to set default', 'Close', { duration: 3000 }),
    });
  }

  onFeatureToggle(provider: AIProvider, featureKey: string, checked: boolean) {
    const mapping: FeatureMapping = { ...provider.feature_mapping, [featureKey]: checked };
    this.providerService.updateFeatures(provider.id, mapping).subscribe({
      next: () => this.loadProviders(),
      error: () => this.snackBar.open('Failed to update features', 'Close', { duration: 3000 }),
    });
  }

  getPreset(providerName: string): ProviderPreset | undefined {
    return this.presets.find(p => p.provider_name === providerName);
  }

  getModelSuggestions(): string[] {
    const preset = this.presets.find(p => p.provider_name === this.formData.provider_name);
    return preset?.model_suggestions || [];
  }

  isAzure(): boolean {
    return this.formData.provider_name === 'azure_openai';
  }

  formatTokens(tokens: number): string {
    if (tokens >= 1000000) return (tokens / 1000000).toFixed(1) + 'M';
    if (tokens >= 1000) return (tokens / 1000).toFixed(1) + 'K';
    return tokens.toString();
  }

  goBack() {
    this.router.navigate(['/tender-dashboard']);
  }
}
