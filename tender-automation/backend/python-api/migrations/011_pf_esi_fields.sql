-- Migration 011: Add PF & ESI registration fields to company_profiles
-- Date: 2026-03-05

ALTER TABLE company_profiles ADD pf_registration_number NVARCHAR(100) NULL;
ALTER TABLE company_profiles ADD pf_registration_date DATETIMEOFFSET NULL;
ALTER TABLE company_profiles ADD esi_registration_number NVARCHAR(100) NULL;
ALTER TABLE company_profiles ADD esi_registration_date DATETIMEOFFSET NULL;
