import { Injectable, signal, computed } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

// Theme configuration interfaces
export interface ThemeConfig {
  theme: 'light' | 'dark' | 'auto';
  fontSize: 'xs' | 'sm' | 'base' | 'lg' | 'xl';
  fontFamily: 'inter' | 'roboto' | 'system';
  language: 'en' | 'hi' | 'bn' | 'ta' | 'te';
  layout: 'default' | 'compact' | 'spacious';
  sidebarCollapsed: boolean;
  primaryColor: string;
  secondaryColor: string;
  successColor: string;
  dangerColor: string;
  warningColor: string;
  infoColor: string;
  textPrimaryColor: string;
  textSecondaryColor: string;
  backgroundColor: string;
  surfaceColor: string;
  borderColor: string;
  borderRadius: 'none' | 'sm' | 'md' | 'lg' | 'xl';
  animationsEnabled: boolean;
  highContrast: boolean;
}

export interface FontSizeOption {
  value: ThemeConfig['fontSize'];
  label: string;
  description: string;
  cssClass: string;
}

export interface FontFamilyOption {
  value: ThemeConfig['fontFamily'];
  label: string;
  description: string;
  cssClass: string;
}

export interface LanguageOption {
  value: ThemeConfig['language'];
  label: string;
  nativeName: string;
  flag: string;
}

export interface LayoutOption {
  value: ThemeConfig['layout'];
  label: string;
  description: string;
  spacing: string;
}

export interface ColorOption {
  value: string;
  label: string;
  color: string;
}

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  // Local storage keys
  private readonly STORAGE_KEY = 'cms-theme-config';
  private readonly LANGUAGE_KEY = 'cms-language';

  // Default theme configuration
  private readonly defaultConfig: ThemeConfig = {
    theme: 'light',
    fontSize: 'base',
    fontFamily: 'inter',
    language: 'en',
    layout: 'default',
    sidebarCollapsed: false,
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
    borderColor: '#e5e7eb',
    borderRadius: 'lg',
    animationsEnabled: true,
    highContrast: false
  };

  // Reactive signals
  private configSignal = signal<ThemeConfig>(this.defaultConfig);
  private configSubject = new BehaviorSubject<ThemeConfig>(this.defaultConfig);

  // Public observables and computed values
  public config$ = this.configSubject.asObservable();
  public currentTheme = computed(() => this.configSignal().theme);
  public currentFontSize = computed(() => this.configSignal().fontSize);
  public currentFontFamily = computed(() => this.configSignal().fontFamily);
  public currentLanguage = computed(() => this.configSignal().language);
  public currentLayout = computed(() => this.configSignal().layout);
  public sidebarCollapsed = computed(() => this.configSignal().sidebarCollapsed);

  constructor() {
    this.loadFromStorage();
    this.applyTheme();
  }

  // Get current configuration
  get config(): ThemeConfig {
    return this.configSignal();
  }

  // Update configuration
  updateConfig(updates: Partial<ThemeConfig>): void {
    const newConfig = { ...this.configSignal(), ...updates };
    this.configSignal.set(newConfig);
    this.configSubject.next(newConfig);
    this.saveToStorage();
    this.applyTheme();
  }

  // Theme management
  setTheme(theme: ThemeConfig['theme']): void {
    this.updateConfig({ theme });
  }

  toggleTheme(): void {
    const currentTheme = this.configSignal().theme;
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';
    this.setTheme(newTheme);
  }

  setAutoTheme(): void {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    this.setTheme(prefersDark ? 'dark' : 'light');

    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
      if (this.configSignal().theme === 'auto') {
        this.setTheme(e.matches ? 'dark' : 'light');
      }
    });
  }

  // Font size management
  setFontSize(fontSize: ThemeConfig['fontSize']): void {
    this.updateConfig({ fontSize });
  }

  increaseFontSize(): void {
    const sizes: ThemeConfig['fontSize'][] = ['xs', 'sm', 'base', 'lg', 'xl'];
    const current = this.configSignal().fontSize;
    const currentIndex = sizes.indexOf(current);
    if (currentIndex < sizes.length - 1) {
      this.setFontSize(sizes[currentIndex + 1]);
    }
  }

  decreaseFontSize(): void {
    const sizes: ThemeConfig['fontSize'][] = ['xs', 'sm', 'base', 'lg', 'xl'];
    const current = this.configSignal().fontSize;
    const currentIndex = sizes.indexOf(current);
    if (currentIndex > 0) {
      this.setFontSize(sizes[currentIndex - 1]);
    }
  }

  // Font family management
  setFontFamily(fontFamily: ThemeConfig['fontFamily']): void {
    this.updateConfig({ fontFamily });
  }

  // Language management
  setLanguage(language: ThemeConfig['language']): void {
    this.updateConfig({ language });
    localStorage.setItem(this.LANGUAGE_KEY, language);
  }

  // Layout management
  setLayout(layout: ThemeConfig['layout']): void {
    this.updateConfig({ layout });
  }

  toggleSidebar(): void {
    const currentCollapsed = this.configSignal().sidebarCollapsed;
    this.updateConfig({ sidebarCollapsed: !currentCollapsed });
  }

  setSidebarCollapsed(collapsed: boolean): void {
    this.updateConfig({ sidebarCollapsed: collapsed });
  }

  // Color management methods
  setPrimaryColor(color: string): void {
    this.updateConfig({ primaryColor: color });
  }

  setSecondaryColor(color: string): void {
    this.updateConfig({ secondaryColor: color });
  }

  setSuccessColor(color: string): void {
    this.updateConfig({ successColor: color });
  }

  setDangerColor(color: string): void {
    this.updateConfig({ dangerColor: color });
  }

  setWarningColor(color: string): void {
    this.updateConfig({ warningColor: color });
  }

  setInfoColor(color: string): void {
    this.updateConfig({ infoColor: color });
  }

  setTextPrimaryColor(color: string): void {
    this.updateConfig({ textPrimaryColor: color });
  }

  setTextSecondaryColor(color: string): void {
    this.updateConfig({ textSecondaryColor: color });
  }

  setBackgroundColor(color: string): void {
    this.updateConfig({ backgroundColor: color });
  }

  setSurfaceColor(color: string): void {
    this.updateConfig({ surfaceColor: color });
  }

  setBorderColor(color: string): void {
    this.updateConfig({ borderColor: color });
  }

  // Set custom color by property name
  setCustomColor(property: keyof ThemeConfig, color: string): void {
    if (property.toLowerCase().includes('color')) {
      this.updateConfig({ [property]: color } as Partial<ThemeConfig>);
    }
  }

  // Get all customizable colors
  getColorPalette(): Array<{key: keyof ThemeConfig; label: string; value: string}> {
    const config = this.configSignal();
    return [
      { key: 'primaryColor', label: 'Primary Color', value: config.primaryColor },
      { key: 'secondaryColor', label: 'Secondary Color', value: config.secondaryColor },
      { key: 'successColor', label: 'Success Color', value: config.successColor },
      { key: 'dangerColor', label: 'Danger Color', value: config.dangerColor },
      { key: 'warningColor', label: 'Warning Color', value: config.warningColor },
      { key: 'infoColor', label: 'Info Color', value: config.infoColor },
      { key: 'textPrimaryColor', label: 'Primary Text', value: config.textPrimaryColor },
      { key: 'textSecondaryColor', label: 'Secondary Text', value: config.textSecondaryColor },
      { key: 'backgroundColor', label: 'Background', value: config.backgroundColor },
      { key: 'surfaceColor', label: 'Surface/Card', value: config.surfaceColor },
      { key: 'borderColor', label: 'Border', value: config.borderColor }
    ];
  }

  // Border radius management
  setBorderRadius(radius: ThemeConfig['borderRadius']): void {
    this.updateConfig({ borderRadius: radius });
  }

  // Accessibility features
  toggleAnimations(): void {
    const current = this.configSignal().animationsEnabled;
    this.updateConfig({ animationsEnabled: !current });
  }

  toggleHighContrast(): void {
    const current = this.configSignal().highContrast;
    this.updateConfig({ highContrast: !current });
  }

  // Configuration options
  get fontSizeOptions(): FontSizeOption[] {
    return [
      {
        value: 'xs',
        label: 'Extra Small',
        description: '12px - Best for compact layouts',
        cssClass: 'font-size-xs'
      },
      {
        value: 'sm',
        label: 'Small',
        description: '14px - Good for dense information',
        cssClass: 'font-size-sm'
      },
      {
        value: 'base',
        label: 'Normal',
        description: '16px - Default size',
        cssClass: 'font-size-base'
      },
      {
        value: 'lg',
        label: 'Large',
        description: '18px - Better readability',
        cssClass: 'font-size-lg'
      },
      {
        value: 'xl',
        label: 'Extra Large',
        description: '20px - Maximum readability',
        cssClass: 'font-size-xl'
      }
    ];
  }

  get fontFamilyOptions(): FontFamilyOption[] {
    return [
      {
        value: 'inter',
        label: 'Inter',
        description: 'Modern, clean sans-serif',
        cssClass: 'font-inter'
      },
      {
        value: 'roboto',
        label: 'Roboto',
        description: 'Material Design font',
        cssClass: 'font-roboto'
      },
      {
        value: 'system',
        label: 'System',
        description: 'Operating system default',
        cssClass: 'font-system'
      }
    ];
  }

  get languageOptions(): LanguageOption[] {
    return [
      {
        value: 'en',
        label: 'English',
        nativeName: 'English',
        flag: '🇺🇸'
      },
      {
        value: 'hi',
        label: 'Hindi',
        nativeName: 'हिन्दी',
        flag: '🇮🇳'
      },
      {
        value: 'bn',
        label: 'Bengali',
        nativeName: 'বাংলা',
        flag: '🇧🇩'
      },
      {
        value: 'ta',
        label: 'Tamil',
        nativeName: 'தமிழ்',
        flag: '🇱🇰'
      },
      {
        value: 'te',
        label: 'Telugu',
        nativeName: 'తెలుగు',
        flag: '🇮🇳'
      }
    ];
  }

  get layoutOptions(): LayoutOption[] {
    return [
      {
        value: 'compact',
        label: 'Compact',
        description: 'Dense layout for maximum content',
        spacing: 'var(--spacing-2)'
      },
      {
        value: 'default',
        label: 'Default',
        description: 'Balanced layout',
        spacing: 'var(--spacing-4)'
      },
      {
        value: 'spacious',
        label: 'Spacious',
        description: 'Roomy layout for comfort',
        spacing: 'var(--spacing-6)'
      }
    ];
  }

  get colorOptions(): ColorOption[] {
    return [
      { value: '#2563eb', label: 'Blue', color: '#2563eb' },
      { value: '#dc2626', label: 'Red', color: '#dc2626' },
      { value: '#16a34a', label: 'Green', color: '#16a34a' },
      { value: '#d97706', label: 'Orange', color: '#d97706' },
      { value: '#7c3aed', label: 'Purple', color: '#7c3aed' },
      { value: '#0891b2', label: 'Cyan', color: '#0891b2' },
      { value: '#e11d48', label: 'Pink', color: '#e11d48' },
      { value: '#64748b', label: 'Slate', color: '#64748b' }
    ];
  }

  get borderRadiusOptions(): Array<{value: ThemeConfig['borderRadius']; label: string; cssValue: string}> {
    return [
      { value: 'none', label: 'None', cssValue: '0' },
      { value: 'sm', label: 'Small', cssValue: '0.25rem' },
      { value: 'md', label: 'Medium', cssValue: '0.5rem' },
      { value: 'lg', label: 'Large', cssValue: '0.75rem' },
      { value: 'xl', label: 'Extra Large', cssValue: '1rem' }
    ];
  }

  // Apply theme to DOM
  private applyTheme(): void {
    const config = this.configSignal();
    const root = document.documentElement;

    // Apply theme (light/dark)
    if (config.theme === 'auto') {
      this.setAutoTheme();
    } else {
      root.setAttribute('data-theme', config.theme);
    }

    // Apply font size
    root.style.setProperty('--app-font-size', this.getFontSizeValue(config.fontSize));

    // Apply font family
    root.style.setProperty('--app-font-family', this.getFontFamilyValue(config.fontFamily));

    // Apply layout spacing
    root.style.setProperty('--app-spacing', this.getLayoutSpacing(config.layout));

    // Apply all custom colors
    root.style.setProperty('--color-primary', config.primaryColor);
    root.style.setProperty('--color-primary-hover', this.adjustColor(config.primaryColor, -10));
    root.style.setProperty('--color-primary-light', this.adjustColor(config.primaryColor, 40));
    root.style.setProperty('--color-primary-dark', this.adjustColor(config.primaryColor, -20));

    root.style.setProperty('--color-secondary', config.secondaryColor);
    root.style.setProperty('--color-success', config.successColor);
    root.style.setProperty('--color-danger', config.dangerColor);
    root.style.setProperty('--color-warning', config.warningColor);
    root.style.setProperty('--color-info', config.infoColor);

    root.style.setProperty('--text-primary', config.textPrimaryColor);
    root.style.setProperty('--text-secondary', config.textSecondaryColor);

    root.style.setProperty('--bg-primary', config.backgroundColor);
    root.style.setProperty('--bg-surface', config.surfaceColor);
    root.style.setProperty('--border-color', config.borderColor);

    // Legacy support for old variable names
    root.style.setProperty('--primary-color', config.primaryColor);
    root.style.setProperty('--primary-color-hover', this.adjustColor(config.primaryColor, -10));
    root.style.setProperty('--primary-color-light', this.adjustColor(config.primaryColor, 40));

    // Apply border radius
    root.style.setProperty('--app-border-radius', this.getBorderRadiusValue(config.borderRadius));

    // Apply accessibility settings
    if (config.highContrast) {
      root.classList.add('high-contrast');
    } else {
      root.classList.remove('high-contrast');
    }

    if (!config.animationsEnabled) {
      root.style.setProperty('--transition-base', '0s');
      root.style.setProperty('--transition-fast', '0s');
      root.style.setProperty('--transition-slow', '0s');
      root.classList.add('no-animations');
    } else {
      root.style.removeProperty('--transition-base');
      root.style.removeProperty('--transition-fast');
      root.style.removeProperty('--transition-slow');
      root.classList.remove('no-animations');
    }

    // Apply RTL for specific languages
    if (['ar', 'he', 'fa'].includes(config.language)) {
      root.setAttribute('dir', 'rtl');
    } else {
      root.setAttribute('dir', 'ltr');
    }

    // Set language attribute
    root.setAttribute('lang', config.language);
  }

  // Helper methods
  private getFontSizeValue(size: ThemeConfig['fontSize']): string {
    const sizes = {
      xs: '14px',
      sm: '15px',
      base: '16px',
      lg: '18px',
      xl: '20px'
    };
    return sizes[size];
  }

  private getFontFamilyValue(family: ThemeConfig['fontFamily']): string {
    const families = {
      inter: "'Inter', 'Roboto', sans-serif",
      roboto: "'Roboto', 'Arial', sans-serif",
      system: "system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif"
    };
    return families[family];
  }

  private getLayoutSpacing(layout: ThemeConfig['layout']): string {
    const spacing = {
      compact: 'var(--spacing-2)',
      default: 'var(--spacing-4)',
      spacious: 'var(--spacing-6)'
    };
    return spacing[layout];
  }

  private getBorderRadiusValue(radius: ThemeConfig['borderRadius']): string {
    const values = {
      none: '0',
      sm: '0.25rem',
      md: '0.5rem',
      lg: '0.75rem',
      xl: '1rem'
    };
    return values[radius];
  }

  private adjustColor(color: string, amount: number): string {
    const clamp = (num: number) => Math.min(255, Math.max(0, num));
    const hex = color.replace('#', '');
    const r = clamp(parseInt(hex.substr(0, 2), 16) + amount);
    const g = clamp(parseInt(hex.substr(2, 2), 16) + amount);
    const b = clamp(parseInt(hex.substr(4, 2), 16) + amount);
    return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
  }

  // Storage management
  private saveToStorage(): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.configSignal()));
    } catch (error) {
      console.warn('Failed to save theme configuration to localStorage:', error);
    }
  }

  private loadFromStorage(): void {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (stored) {
        const parsedConfig = JSON.parse(stored);
        const mergedConfig = { ...this.defaultConfig, ...parsedConfig };
        this.configSignal.set(mergedConfig);
        this.configSubject.next(mergedConfig);
      }

      // Load language separately
      const storedLanguage = localStorage.getItem(this.LANGUAGE_KEY);
      if (storedLanguage) {
        this.setLanguage(storedLanguage as ThemeConfig['language']);
      }
    } catch (error) {
      console.warn('Failed to load theme configuration from localStorage:', error);
    }
  }

  // Reset to defaults
  resetToDefaults(): void {
    this.configSignal.set(this.defaultConfig);
    this.configSubject.next(this.defaultConfig);
    this.saveToStorage();
    this.applyTheme();
  }

  // Export/Import configuration
  exportConfig(): string {
    return JSON.stringify(this.configSignal(), null, 2);
  }

  importConfig(configJson: string): boolean {
    try {
      const parsedConfig = JSON.parse(configJson);
      const validatedConfig = { ...this.defaultConfig, ...parsedConfig };
      this.configSignal.set(validatedConfig);
      this.configSubject.next(validatedConfig);
      this.saveToStorage();
      this.applyTheme();
      return true;
    } catch (error) {
      console.error('Failed to import theme configuration:', error);
      return false;
    }
  }
}