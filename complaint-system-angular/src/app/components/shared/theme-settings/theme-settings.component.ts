import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService, ThemeConfig } from '../../../services/theme.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-theme-settings',
  imports: [CommonModule],
  templateUrl: './theme-settings.component.html',
  styleUrls: ['./theme-settings.component.scss']
})
export class ThemeSettingsComponent implements OnInit, OnDestroy {
  private subscriptions = new Subscription();

  // Current configuration
  currentConfig: ThemeConfig;

  // UI state
  isExpanded = false;
  activeTab: 'appearance' | 'accessibility' | 'advanced' = 'appearance';

  // Available options - declare without initialization
  fontSizeOptions: any[];
  fontFamilyOptions: any[];
  languageOptions: any[];
  layoutOptions: any[];
  colorOptions: any[];
  borderRadiusOptions: any[];

  constructor(private themeService: ThemeService) {
    this.currentConfig = this.themeService.config;

    // Initialize options from theme service in constructor
    this.fontSizeOptions = this.themeService.fontSizeOptions;
    this.fontFamilyOptions = this.themeService.fontFamilyOptions;
    this.languageOptions = this.themeService.languageOptions;
    this.layoutOptions = this.themeService.layoutOptions;
    this.colorOptions = this.themeService.colorOptions;
    this.borderRadiusOptions = this.themeService.borderRadiusOptions;
  }

  ngOnInit(): void {
    this.subscriptions.add(
      this.themeService.config$.subscribe(config => {
        this.currentConfig = config;
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  // UI helpers
  toggleExpanded(): void {
    this.isExpanded = !this.isExpanded;
  }

  setActiveTab(tab: typeof this.activeTab): void {
    this.activeTab = tab;
  }

  // Theme management
  setTheme(theme: ThemeConfig['theme']): void {
    this.themeService.setTheme(theme);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  // Font settings
  setFontSize(fontSize: ThemeConfig['fontSize']): void {
    this.themeService.setFontSize(fontSize);
  }

  setFontFamily(fontFamily: ThemeConfig['fontFamily']): void {
    this.themeService.setFontFamily(fontFamily);
  }

  increaseFontSize(): void {
    this.themeService.increaseFontSize();
  }

  decreaseFontSize(): void {
    this.themeService.decreaseFontSize();
  }

  // Language settings
  setLanguage(language: ThemeConfig['language']): void {
    this.themeService.setLanguage(language);
  }

  // Layout settings
  setLayout(layout: ThemeConfig['layout']): void {
    this.themeService.setLayout(layout);
  }

  toggleSidebar(): void {
    this.themeService.toggleSidebar();
  }

  setSidebarCollapsed(collapsed: boolean): void {
    this.themeService.setSidebarCollapsed(collapsed);
  }

  // Color settings
  setPrimaryColor(color: string): void {
    this.themeService.setPrimaryColor(color);
  }

  // Border radius settings
  setBorderRadius(radius: ThemeConfig['borderRadius']): void {
    this.themeService.setBorderRadius(radius);
  }

  // Accessibility settings
  toggleAnimations(): void {
    this.themeService.toggleAnimations();
  }

  toggleHighContrast(): void {
    this.themeService.toggleHighContrast();
  }

  // Reset functionality
  resetToDefaults(): void {
    if (confirm('Are you sure you want to reset all theme settings to defaults?')) {
      this.themeService.resetToDefaults();
    }
  }

  // Export/Import functionality
  exportConfig(): void {
    const config = this.themeService.exportConfig();
    const blob = new Blob([config], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'theme-config.json';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  importConfig(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      const content = e.target?.result as string;
      if (this.themeService.importConfig(content)) {
        alert('Theme configuration imported successfully!');
      } else {
        alert('Failed to import theme configuration. Please check the file format.');
      }
    };
    reader.readAsText(file);
  }

  // Quick actions
  applyQuickTheme(themeName: string): void {
    switch (themeName) {
      case 'light':
        this.themeService.updateConfig({
          theme: 'light',
          primaryColor: '#2563eb',
          fontSize: 'base',
          fontFamily: 'inter'
        });
        break;
      case 'dark':
        this.themeService.updateConfig({
          theme: 'dark',
          primaryColor: '#3b82f6',
          fontSize: 'base',
          fontFamily: 'inter'
        });
        break;
      case 'high-contrast':
        this.themeService.updateConfig({
          theme: 'light',
          highContrast: true,
          fontSize: 'lg',
          fontFamily: 'system',
          primaryColor: '#000000'
        });
        break;
      case 'accessibility':
        this.themeService.updateConfig({
          theme: 'light',
          fontSize: 'xl',
          highContrast: false,
          animationsEnabled: false,
          borderRadius: 'lg',
          fontFamily: 'system'
        });
        break;
    }
  }

  // Get display helpers
  getThemeIcon(theme: ThemeConfig['theme']): string {
    switch (theme) {
      case 'light': return 'bi-sun';
      case 'dark': return 'bi-moon';
      case 'auto': return 'bi-circle-half';
      default: return 'bi-palette';
    }
  }

  getLanguageFlag(language: ThemeConfig['language']): string {
    const option = this.languageOptions.find(opt => opt.value === language);
    return option?.flag || '🌐';
  }

  getCurrentThemeLabel(): string {
    const theme = this.currentConfig.theme;
    switch (theme) {
      case 'light': return 'Light';
      case 'dark': return 'Dark';
      case 'auto': return 'Auto';
      default: return 'Light';
    }
  }

  getCurrentFontSizeLabel(): string {
    const option = this.fontSizeOptions.find(opt => opt.value === this.currentConfig.fontSize);
    return option?.label || 'Normal';
  }

  getCurrentFontFamilyLabel(): string {
    const option = this.fontFamilyOptions.find(opt => opt.value === this.currentConfig.fontFamily);
    return option?.label || 'Inter';
  }

  getCurrentLanguageLabel(): string {
    const option = this.languageOptions.find(opt => opt.value === this.currentConfig.language);
    return option?.label || 'English';
  }

  getCurrentLayoutLabel(): string {
    const option = this.layoutOptions.find(opt => opt.value === this.currentConfig.layout);
    return option?.label || 'Default';
  }
}