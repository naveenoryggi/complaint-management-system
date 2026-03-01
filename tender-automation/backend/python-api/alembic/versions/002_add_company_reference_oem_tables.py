"""Add company, reference bundle, OEM, and tracking tables

Revision ID: 002
Revises: 001
Create Date: 2025-02-24

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa
from sqlalchemy.dialects import postgresql

# revision identifiers, used by Alembic.
revision: str = '002'
down_revision: Union[str, None] = '001'
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    """Create company, reference bundle, OEM, and tracking tables."""

    # -----------------------------------------------------------------------
    # 1. company_profiles
    # -----------------------------------------------------------------------
    op.create_table(
        'company_profiles',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('tenant_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('company_name', sa.String(length=500), nullable=False),
        sa.Column('cin_number', sa.String(length=50), nullable=True),
        sa.Column('pan_number', sa.String(length=20), nullable=True),
        sa.Column('gstin', sa.String(length=20), nullable=True),
        sa.Column('msme_registration', sa.String(length=50), nullable=True),
        sa.Column('registered_address', sa.Text(), nullable=True),
        sa.Column('corporate_address', sa.Text(), nullable=True),
        sa.Column('website', sa.String(length=300), nullable=True),
        sa.Column('phone', sa.String(length=20), nullable=True),
        sa.Column('email', sa.String(length=255), nullable=True),
        sa.Column('annual_turnover', postgresql.JSONB(astext_type=sa.Text()), nullable=True),
        sa.Column('year_established', sa.Integer(), nullable=True),
        sa.Column('employee_count', sa.Integer(), nullable=True),
        sa.Column('logo_path', sa.String(length=500), nullable=True),
        sa.Column('letterhead_path', sa.String(length=500), nullable=True),
        sa.Column('signature_path', sa.String(length=500), nullable=True),
        sa.Column('stamp_path', sa.String(length=500), nullable=True),
        sa.Column('bank_name', sa.String(length=200), nullable=True),
        sa.Column('account_number', sa.String(length=50), nullable=True),
        sa.Column('ifsc_code', sa.String(length=20), nullable=True),
        sa.Column('branch_name', sa.String(length=200), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_company_profiles_tenant', 'company_profiles', ['tenant_id'], unique=True)

    # -----------------------------------------------------------------------
    # 2. certifications
    # -----------------------------------------------------------------------
    op.create_table(
        'certifications',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('company_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('name', sa.String(length=300), nullable=False),
        sa.Column('cert_type', sa.String(length=100), nullable=True),
        sa.Column('issuing_body', sa.String(length=300), nullable=True),
        sa.Column('certificate_number', sa.String(length=100), nullable=True),
        sa.Column('issue_date', sa.DateTime(timezone=True), nullable=True),
        sa.Column('expiry_date', sa.DateTime(timezone=True), nullable=True),
        sa.Column('document_path', sa.String(length=500), nullable=True),
        sa.Column('is_valid', sa.Boolean(), nullable=False, server_default='true'),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.ForeignKeyConstraint(['company_id'], ['company_profiles.id'], ondelete='CASCADE'),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_certifications_company', 'certifications', ['company_id'])

    # -----------------------------------------------------------------------
    # 3. personnel
    # -----------------------------------------------------------------------
    op.create_table(
        'personnel',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('company_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('name', sa.String(length=300), nullable=False),
        sa.Column('designation', sa.String(length=200), nullable=True),
        sa.Column('role_in_tender', sa.String(length=200), nullable=True),
        sa.Column('qualification', sa.String(length=300), nullable=True),
        sa.Column('experience_years', sa.Integer(), nullable=True),
        sa.Column('specialization', postgresql.ARRAY(sa.String()), nullable=True),
        sa.Column('cv_path', sa.String(length=500), nullable=True),
        sa.Column('experience_cert_path', sa.String(length=500), nullable=True),
        sa.Column('qualification_cert_path', sa.String(length=500), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.ForeignKeyConstraint(['company_id'], ['company_profiles.id'], ondelete='CASCADE'),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_personnel_company', 'personnel', ['company_id'])

    # -----------------------------------------------------------------------
    # 4. reference_bundles
    # -----------------------------------------------------------------------
    op.create_table(
        'reference_bundles',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('tenant_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('bundle_name', sa.String(length=500), nullable=False),
        sa.Column('client_name', sa.String(length=500), nullable=False),
        sa.Column('client_short_name', sa.String(length=100), nullable=True),
        sa.Column('client_type', sa.String(length=50), nullable=True),
        sa.Column('project_name', sa.String(length=500), nullable=True),
        sa.Column('work_order_number', sa.String(length=100), nullable=True),
        sa.Column('work_order_date', sa.Date(), nullable=True),
        sa.Column('contract_value', sa.Float(), nullable=True),
        sa.Column('gst_value', sa.Float(), nullable=True),
        sa.Column('total_value', sa.Float(), nullable=True),
        sa.Column('duration_months', sa.Integer(), nullable=True),
        sa.Column('start_date', sa.Date(), nullable=True),
        sa.Column('actual_completion_date', sa.Date(), nullable=True),
        sa.Column('status', sa.String(length=50), nullable=False, server_default='completed'),
        sa.Column('scope_description', sa.Text(), nullable=True),
        sa.Column('value_bands', postgresql.ARRAY(sa.String()), nullable=True),
        sa.Column('client_type_tags', postgresql.ARRAY(sa.String()), nullable=True),
        sa.Column('work_type_tags', postgresql.ARRAY(sa.String()), nullable=True),
        sa.Column('completeness_score', sa.Integer(), nullable=False, server_default='0'),
        sa.Column('missing_documents', postgresql.ARRAY(sa.String()), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_reference_bundles_tenant', 'reference_bundles', ['tenant_id'])

    # -----------------------------------------------------------------------
    # 5. bundle_documents
    # -----------------------------------------------------------------------
    op.create_table(
        'bundle_documents',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('bundle_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('tier', sa.String(length=50), nullable=False),
        sa.Column('doc_subtype', sa.String(length=100), nullable=False),
        sa.Column('document_path', sa.String(length=500), nullable=True),
        sa.Column('file_name', sa.String(length=300), nullable=True),
        sa.Column('invoice_number', sa.String(length=100), nullable=True),
        sa.Column('invoice_date', sa.Date(), nullable=True),
        sa.Column('invoice_amount', sa.Float(), nullable=True),
        sa.Column('signatory_name', sa.String(length=200), nullable=True),
        sa.Column('signatory_designation', sa.String(length=200), nullable=True),
        sa.Column('is_verified', sa.Boolean(), nullable=False, server_default='false'),
        sa.Column('notes', sa.Text(), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.ForeignKeyConstraint(['bundle_id'], ['reference_bundles.id'], ondelete='CASCADE'),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_bundle_documents_bundle', 'bundle_documents', ['bundle_id'])

    # -----------------------------------------------------------------------
    # 6. tender_criteria
    # -----------------------------------------------------------------------
    op.create_table(
        'tender_criteria',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('tender_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('criteria_code', sa.String(length=50), nullable=False),
        sa.Column('description', sa.Text(), nullable=True),
        sa.Column('stage', sa.String(length=20), nullable=False, server_default='marking'),
        sa.Column('max_marks', sa.Float(), nullable=False, server_default='0'),
        sa.Column('qualifying_marks', sa.Float(), nullable=True),
        sa.Column('scoring_rules', postgresql.JSONB(astext_type=sa.Text()), nullable=True),
        sa.Column('evidence_required', postgresql.ARRAY(sa.String()), nullable=True),
        sa.Column('can_reuse_across_tenders', sa.Boolean(), nullable=False, server_default='true'),
        sa.Column('max_reuse_count', sa.Integer(), nullable=True),
        sa.Column('ai_extracted', sa.Boolean(), nullable=False, server_default='false'),
        sa.Column('source_document_id', postgresql.UUID(as_uuid=True), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.ForeignKeyConstraint(['tender_id'], ['tenders.id'], ondelete='CASCADE'),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_tender_criteria_tender', 'tender_criteria', ['tender_id'])

    # -----------------------------------------------------------------------
    # 7. bundle_criteria_assignments
    # -----------------------------------------------------------------------
    op.create_table(
        'bundle_criteria_assignments',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('bundle_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('criteria_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('predicted_marks', sa.Float(), nullable=True),
        sa.Column('actual_marks', sa.Float(), nullable=True),
        sa.Column('submitted_doc_types', postgresql.ARRAY(sa.String()), nullable=True),
        sa.Column('notes', sa.Text(), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.ForeignKeyConstraint(['bundle_id'], ['reference_bundles.id'], ondelete='CASCADE'),
        sa.ForeignKeyConstraint(['criteria_id'], ['tender_criteria.id'], ondelete='CASCADE'),
        sa.PrimaryKeyConstraint('id'),
        sa.UniqueConstraint('bundle_id', 'criteria_id', name='uq_bundle_criteria'),
    )
    op.create_index('idx_bca_bundle', 'bundle_criteria_assignments', ['bundle_id'])
    op.create_index('idx_bca_criteria', 'bundle_criteria_assignments', ['criteria_id'])

    # -----------------------------------------------------------------------
    # 8. oem_master
    # -----------------------------------------------------------------------
    op.create_table(
        'oem_master',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('tenant_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('name', sa.String(length=300), nullable=False),
        sa.Column('country', sa.String(length=100), nullable=True),
        sa.Column('is_indian', sa.Boolean(), nullable=False, server_default='true'),
        sa.Column('india_distributor', sa.String(length=300), nullable=True),
        sa.Column('partner_tier', sa.String(length=50), nullable=True),
        sa.Column('partner_certificate_path', sa.String(length=500), nullable=True),
        sa.Column('am_contact_name', sa.String(length=200), nullable=True),
        sa.Column('am_contact_email', sa.String(length=255), nullable=True),
        sa.Column('am_contact_phone', sa.String(length=20), nullable=True),
        sa.Column('legal_contact_name', sa.String(length=200), nullable=True),
        sa.Column('legal_contact_email', sa.String(length=255), nullable=True),
        sa.Column('product_categories', postgresql.ARRAY(sa.String()), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_oem_master_tenant', 'oem_master', ['tenant_id'])

    # -----------------------------------------------------------------------
    # 9. oem_tender_requirements
    # -----------------------------------------------------------------------
    op.create_table(
        'oem_tender_requirements',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('tender_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('oem_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('maf_status', sa.String(length=30), nullable=False, server_default='pending'),
        sa.Column('maf_document_path', sa.String(length=500), nullable=True),
        sa.Column('maf_received_date', sa.Date(), nullable=True),
        sa.Column('ms_status', sa.String(length=30), nullable=False, server_default='pending'),
        sa.Column('ms_document_path', sa.String(length=500), nullable=True),
        sa.Column('partner_cert_status', sa.String(length=30), nullable=False, server_default='pending'),
        sa.Column('partner_cert_path', sa.String(length=500), nullable=True),
        sa.Column('datasheet_status', sa.String(length=30), nullable=False, server_default='pending'),
        sa.Column('datasheet_path', sa.String(length=500), nullable=True),
        sa.Column('compliance_cert_status', sa.String(length=30), nullable=False, server_default='pending'),
        sa.Column('compliance_cert_path', sa.String(length=500), nullable=True),
        sa.Column('notes', sa.Text(), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.ForeignKeyConstraint(['tender_id'], ['tenders.id'], ondelete='CASCADE'),
        sa.ForeignKeyConstraint(['oem_id'], ['oem_master.id'], ondelete='CASCADE'),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_oem_tender_req_tender', 'oem_tender_requirements', ['tender_id'])
    op.create_index('idx_oem_tender_req_oem', 'oem_tender_requirements', ['oem_id'])

    # -----------------------------------------------------------------------
    # 10. portal_registrations
    # -----------------------------------------------------------------------
    op.create_table(
        'portal_registrations',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('tenant_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('portal_name', sa.String(length=200), nullable=False),
        sa.Column('portal_url', sa.String(length=500), nullable=True),
        sa.Column('portal_type', sa.String(length=50), nullable=True),
        sa.Column('registration_status', sa.String(length=30), nullable=False, server_default='active'),
        sa.Column('registration_number', sa.String(length=100), nullable=True),
        sa.Column('registered_email', sa.String(length=255), nullable=True),
        sa.Column('dsc_class', sa.String(length=20), nullable=True),
        sa.Column('dsc_holder_name', sa.String(length=200), nullable=True),
        sa.Column('dsc_expiry_date', sa.Date(), nullable=True),
        sa.Column('vendor_category', sa.String(length=100), nullable=True),
        sa.Column('notes', sa.Text(), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_portal_registrations_tenant', 'portal_registrations', ['tenant_id'])

    # -----------------------------------------------------------------------
    # 11. emd_records
    # -----------------------------------------------------------------------
    op.create_table(
        'emd_records',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('tender_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('amount', sa.Float(), nullable=False),
        sa.Column('mode', sa.String(length=50), nullable=False),
        sa.Column('instrument_number', sa.String(length=100), nullable=True),
        sa.Column('instrument_date', sa.Date(), nullable=True),
        sa.Column('issuing_bank', sa.String(length=200), nullable=True),
        sa.Column('validity_start_date', sa.Date(), nullable=True),
        sa.Column('validity_end_date', sa.Date(), nullable=True),
        sa.Column('submitted_date', sa.Date(), nullable=True),
        sa.Column('released_date', sa.Date(), nullable=True),
        sa.Column('release_amount', sa.Float(), nullable=True),
        sa.Column('status', sa.String(length=30), nullable=False, server_default='pending'),
        sa.Column('notes', sa.Text(), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.ForeignKeyConstraint(['tender_id'], ['tenders.id'], ondelete='CASCADE'),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_emd_records_tender', 'emd_records', ['tender_id'])

    # -----------------------------------------------------------------------
    # 12. tender_fees
    # -----------------------------------------------------------------------
    op.create_table(
        'tender_fees',
        sa.Column('id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('tender_id', postgresql.UUID(as_uuid=True), nullable=False),
        sa.Column('fee_type', sa.String(length=50), nullable=False),
        sa.Column('amount', sa.Float(), nullable=False),
        sa.Column('payment_mode', sa.String(length=50), nullable=True),
        sa.Column('payment_reference', sa.String(length=100), nullable=True),
        sa.Column('payment_date', sa.Date(), nullable=True),
        sa.Column('status', sa.String(length=30), nullable=False, server_default='pending'),
        sa.Column('receipt_path', sa.String(length=500), nullable=True),
        sa.Column('notes', sa.Text(), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('NOW()')),
        sa.ForeignKeyConstraint(['tender_id'], ['tenders.id'], ondelete='CASCADE'),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index('idx_tender_fees_tender', 'tender_fees', ['tender_id'])


def downgrade() -> None:
    """Drop all tables created in this migration."""
    # Drop in reverse order to respect foreign key constraints
    op.drop_index('idx_tender_fees_tender', table_name='tender_fees')
    op.drop_table('tender_fees')

    op.drop_index('idx_emd_records_tender', table_name='emd_records')
    op.drop_table('emd_records')

    op.drop_index('idx_portal_registrations_tenant', table_name='portal_registrations')
    op.drop_table('portal_registrations')

    op.drop_index('idx_oem_tender_req_oem', table_name='oem_tender_requirements')
    op.drop_index('idx_oem_tender_req_tender', table_name='oem_tender_requirements')
    op.drop_table('oem_tender_requirements')

    op.drop_index('idx_oem_master_tenant', table_name='oem_master')
    op.drop_table('oem_master')

    op.drop_index('idx_bca_criteria', table_name='bundle_criteria_assignments')
    op.drop_index('idx_bca_bundle', table_name='bundle_criteria_assignments')
    op.drop_table('bundle_criteria_assignments')

    op.drop_index('idx_tender_criteria_tender', table_name='tender_criteria')
    op.drop_table('tender_criteria')

    op.drop_index('idx_bundle_documents_bundle', table_name='bundle_documents')
    op.drop_table('bundle_documents')

    op.drop_index('idx_reference_bundles_tenant', table_name='reference_bundles')
    op.drop_table('reference_bundles')

    op.drop_index('idx_personnel_company', table_name='personnel')
    op.drop_table('personnel')

    op.drop_index('idx_certifications_company', table_name='certifications')
    op.drop_table('certifications')

    op.drop_index('idx_company_profiles_tenant', table_name='company_profiles')
    op.drop_table('company_profiles')
