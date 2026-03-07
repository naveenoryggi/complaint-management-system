-- Migration 013: Add clause interpretation fields to technical_compliance_items
-- Risk scoring, AI interpretation, and cross-reference detection

ALTER TABLE technical_compliance_items ADD risk_score FLOAT NULL;
ALTER TABLE technical_compliance_items ADD risk_category NVARCHAR(30) NULL;
ALTER TABLE technical_compliance_items ADD risk_reason NVARCHAR(MAX) NULL;
ALTER TABLE technical_compliance_items ADD interpretation NVARCHAR(MAX) NULL;
ALTER TABLE technical_compliance_items ADD cross_references NVARCHAR(MAX) NULL;
