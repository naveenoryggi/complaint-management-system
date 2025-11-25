import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ThemeService, ThemeConfig } from '../../../services/theme.service';

@Component({
  selector: 'app-theme-customizer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './theme-customizer.component.html',
  styleUrl: './theme-customizer.component.scss'
})
export class ThemeCustomizerComponent implements OnInit {
  isOpen = signal(false);
  activeTab = signal<'colors' | 'typography' | 'layout' | 'accessibility'>('colors');

  config!: ThemeConfig;
  colorPalette: Array<{key: keyof ThemeConfig; label: string; value: string}> = [];

  // Preset themes
  presetThemes = [
    {
      name: 'Default',
      config: {
        primaryColor: '#667eea',
        secondaryColor: '#764ba2',
        successColor: '#10b981',
        dangerColor: '#ef4444',
        warningColor: '#f59e0b',
        infoColor: '#3b82f6',
        textPrimaryColor: '#1f2937',
        textSecondaryColor: '#6b7280',
        backgroundColor: '#f9fafb',
        surfaceColor: '#ffffff',
        borderColor: '#e5e7eb'
      }
    },
    {
      name: 'Ocean',
      config: {
        primaryColor: '#0891b2',
        secondaryColor: '#06b6d4',
        successColor: '#10b981',
        dangerColor: '#ef4444',
        warningColor: '#f59e0b',
        infoColor: '#3b82f6',
        textPrimaryColor: '#0f172a',
        textSecondaryColor: '#475569',
        backgroundColor: '#f1f5f9',
        surfaceColor: '#ffffff',
        borderColor: '#cbd5e1'
      }
    },
    {
      name: 'Forest',
      config: {
        primaryColor: '#059669',
        secondaryColor: '#10b981',
        successColor: '#22c55e',
        dangerColor: '#ef4444',
        warningColor: '#f59e0b',
        infoColor: '#3b82f6',
        textPrimaryColor: '#14532d',
        textSecondaryColor: '#166534',
        backgroundColor: '#f0fdf4',
        surfaceColor: '#ffffff',
        borderColor: '#bbf7d0'
      }
    },
    {
      name: 'Sunset',
      config: {
        primaryColor: '#f97316',
        secondaryColor: '#fb923c',
        successColor: '#10b981',
        dangerColor: '#ef4444',
        warningColor: '#f59e0b',
        infoColor: '#3b82f6',
        textPrimaryColor: '#431407',
        textSecondaryColor: '#9a3412',
        backgroundColor: '#fff7ed',
        surfaceColor: '#ffffff',
        borderColor: '#fed7aa'
      }
    },
    {
      name: 'Dark Mode',
      config: {
        primaryColor: '#818cf8',
        secondaryColor: '#a78bfa',
        successColor: '#34d399',
        dangerColor: '#f87171',
        warningColor: '#fbbf24',
        infoColor: '#60a5fa',
        textPrimaryColor: '#f3f4f6',
        textSecondaryColor: '#9ca3af',
        backgroundColor: '#111827',
        surfaceColor: '#1f2937',
        borderColor: '#374151'
      }
    }
  ];

  constructor(public themeService: ThemeService) {}

  ngOnInit(): void {
    this.loadConfig();
  }

  loadConfig(): void {
    this.config = { ...this.themeService.config };
    this.colorPalette = this.themeService.getColorPalette();
  }

  toggleCustomizer(): void {
    this.isOpen.set(!this.isOpen());
  }

  close(): void {
    this.isOpen.set(false);
  }

  setActiveTab(tab: 'colors' | 'typography' | 'layout' | 'accessibility'): void {
    this.activeTab.set(tab);
  }

  // Color management
  onColorChange(key: keyof ThemeConfig, color: string): void {
    this.config = { ...this.config, [key]: color };
    this.themeService.setCustomColor(key, color);
    this.colorPalette = this.themeService.getColorPalette();
  }

  applyPreset(preset: any): void {
    Object.keys(preset.config).forEach(key => {
      this.onColorChange(key as keyof ThemeConfig, preset.config[key]);
    });
  }

  // Typography management
  onFontSizeChange(fontSize: ThemeConfig['fontSize']): void {
    this.themeService.setFontSize(fontSize);
    this.loadConfig();
  }

  onFontFamilyChange(fontFamily: ThemeConfig['fontFamily']): void {
    this.themeService.setFontFamily(fontFamily);
    this.loadConfig();
  }

  // Layout management
  onLayoutChange(layout: ThemeConfig['layout']): void {
    this.themeService.setLayout(layout);
    this.loadConfig();
  }

  onBorderRadiusChange(radius: ThemeConfig['borderRadius']): void {
    this.themeService.setBorderRadius(radius);
    this.loadConfig();
  }

  // Accessibility
  toggleAnimations(): void {
    this.themeService.toggleAnimations();
    this.loadConfig();
  }

  toggleHighContrast(): void {
    this.themeService.toggleHighContrast();
    this.loadConfig();
  }

  // Theme presets
  setTheme(theme: ThemeConfig['theme']): void {
    this.themeService.setTheme(theme);
    this.loadConfig();
  }

  // Reset to defaults
  resetToDefaults(): void {
    if (confirm('Are you sure you want to reset all theme settings to defaults?')) {
      this.themeService.resetToDefaults();
      this.loadConfig();
    }
  }

  // Export/Import
  exportTheme(): void {
    const configJson = this.themeService.exportConfig();
    const blob = new Blob([configJson], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'theme-config.json';
    a.click();
    window.URL.revokeObjectURL(url);
  }

  importTheme(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e) => {
        const content = e.target?.result as string;
        const success = this.themeService.importConfig(content);
        if (success) {
          this.loadConfig();
          alert('Theme imported successfully!');
        } else {
          alert('Failed to import theme. Please check the file format.');
        }
      };
      reader.readAsText(file);
    }
  }

  get fontSizeOptions() {
    return this.themeService.fontSizeOptions;
  }

  get fontFamilyOptions() {
    return this.themeService.fontFamilyOptions;
  }

  get layoutOptions() {
    return this.themeService.layoutOptions;
  }

  get borderRadiusOptions() {
    return this.themeService.borderRadiusOptions;
  }
}
