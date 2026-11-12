// src/pages/public/PrivacyPolicyPage.jsx
import { Shield, Lock, FileText, CheckCircle2, Eye, Globe, Phone, Mail, ArrowLeft } from 'lucide-react';
import { Link } from 'react-router-dom';
import PageWrapper from '@/components/layout/PageWrapper';
import { useCms } from '@/context/CmsContext';

export default function PrivacyPolicyPage() {
  const { navbar } = useCms();
  const companyName = navbar?.companyName || 'AirSystem';

  const sections = [
    {
      id: 'collection',
      icon: Eye,
      title: '1. Information We Collect',
      content: [
        'Personal Identifiers: Full legal name, date of birth, gender, nationality, passport number, national ID, and expiry dates as required by international civil aviation authorities.',
        'Contact Details: Email address, phone number, and physical billing address for sending electronic tickets, flight status updates, and itinerary changes.',
        'Payment Information: Payment card tokens, transaction identifiers, and billing records processed via PCI-DSS Level 1 certified gateways. We do not store raw credit card numbers or security CVV codes on our servers.',
        'Travel & Booking Data: Flight numbers, cabin class, seat preferences, baggage allowances, frequent flyer details, and special service requests.',
      ],
    },
    {
      id: 'usage',
      icon: FileText,
      title: '2. How We Use Your Information',
      content: [
        'Reservation & Ticketing: Processing flight reservations, issuing confirmed e-tickets, assigning seats, and managing check-in services.',
        'Operational Communications: Delivering essential flight notifications, gate changes, departure delays, and digital boarding passes.',
        'Aviation Regulatory Compliance: Submitting Advance Passenger Information (API) and Passenger Name Record (PNR) data to government customs and border protection authorities where legally mandated.',
        'Customer Support & Refunds: Resolving booking inquiries, schedule modifications, baggage claims, and processing authorized refunds.',
      ],
    },
    {
      id: 'security',
      icon: Lock,
      title: '3. Data Security & Encryption',
      content: [
        'End-to-End Encryption: All communications and data transmissions between your browser and our servers are encrypted using modern Transport Layer Security (TLS 1.3 / SSL).',
        'PCI-DSS Compliance: All online payments (Visa, Mastercard, bKash, Nagad) are processed through fully compliant, encrypted, and isolated payment infrastructure.',
        'Access Controls: Strict role-based access control (RBAC) ensures only vetted airline personnel and certified agents access flight passenger manifests for operational purposes.',
        'Database Hardening: Passenger credentials and sensitive data are stored using cryptographic hashing and industry-standard AES-256 database protection.',
      ],
    },
    {
      id: 'sharing',
      icon: Globe,
      title: '4. Sharing of Information',
      content: [
        'Civil Aviation & Border Control: In compliance with international aviation laws (ICAO/IATA), passenger information must be shared with immigration and border security agencies of departure and destination countries.',
        'Codeshare & Interline Airline Partners: If your journey involves connecting segments operated by partner airlines, necessary passenger information is transmitted to ensure seamless flight connectivity.',
        'Authorized Service Providers: Airport ground handling agents, catering providers, and baggage handling services receive minimal data required to fulfill your travel.',
        'No Third-Party Selling: We strictly never sell, lease, or monetize your personal information to third-party advertisers or data brokers.',
      ],
    },
    {
      id: 'rights',
      icon: CheckCircle2,
      title: '5. Passenger Rights (GDPR & Data Protection)',
      content: [
        'Right to Access: You may request a complete copy of the personal and booking data we maintain about you at any time.',
        'Right to Rectification: You can update your profile, contact numbers, and passport information directly through your passenger dashboard.',
        'Right to Erasure: Subject to mandatory aviation record-retention laws, you have the right to request the deletion of your account and personal history.',
        'Marketing Preferences: You can opt out of promotional emails or special offer newsletters with a single click at any time.',
      ],
    },
  ];

  return (
    <PageWrapper>
      <div className="bg-gradient-to-b from-brand-50/60 via-slate-50 to-white py-12">
        <div className="page-container max-w-4xl">
          {/* Back link */}
          <Link
            to="/"
            className="inline-flex items-center gap-2 text-sm font-medium text-slate-500 hover:text-brand-600 transition mb-6"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Flight Search
          </Link>

          {/* Header */}
          <div className="card border-0 shadow-lg bg-gradient-to-r from-brand-700 via-brand-600 to-emerald-600 text-white p-8 md:p-10 mb-8 rounded-3xl">
            <div className="flex items-center gap-3 mb-3">
              <div className="h-12 w-12 rounded-2xl bg-white/15 backdrop-blur-sm flex items-center justify-center">
                <Shield className="h-6 w-6 text-white" />
              </div>
              <span className="text-xs font-semibold tracking-wider uppercase bg-white/20 px-3 py-1 rounded-full">
                Security & Compliance
              </span>
            </div>
            <h1 className="text-3xl md:text-4xl font-extrabold tracking-tight mb-2">
              Privacy Policy & Data Protection
            </h1>
            <p className="text-brand-100 text-base max-w-2xl leading-relaxed">
              At {companyName}, your privacy and security are paramount. This policy outlines how we collect, handle, protect, and respect your passenger information across all flight bookings and services.
            </p>
            <div className="mt-6 pt-4 border-t border-white/20 flex flex-wrap items-center gap-4 text-xs text-brand-100">
              <span>Effective Date: November 2026</span>
              <span>•</span>
              <span>Compliant with IATA, ICAO & Global Data Regulations</span>
            </div>
          </div>

          {/* Policy Sections */}
          <div className="flex flex-col gap-6">
            {sections.map((section) => {
              const Icon = section.icon;
              return (
                <div key={section.id} className="card hover:shadow-md transition">
                  <div className="flex items-center gap-3.5 mb-4">
                    <div className="h-10 w-10 rounded-xl bg-brand-50 text-brand-600 flex items-center justify-center flex-shrink-0">
                      <Icon className="h-5 w-5" />
                    </div>
                    <h2 className="text-xl font-bold text-slate-800">{section.title}</h2>
                  </div>
                  <ul className="space-y-2.5 pl-2">
                    {section.content.map((point, idx) => (
                      <li key={idx} className="flex items-start gap-2.5 text-sm text-slate-600 leading-relaxed">
                        <span className="h-1.5 w-1.5 rounded-full bg-brand-500 mt-2 flex-shrink-0" />
                        <span>{point}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              );
            })}

            {/* Contact / Inquiries Box */}
            <div className="card bg-slate-900 text-white p-8 rounded-3xl mt-4">
              <h3 className="text-xl font-bold mb-2">Questions Regarding Your Privacy?</h3>
              <p className="text-sm text-slate-400 mb-6 leading-relaxed">
                If you have inquiries about our data practices, or wish to exercise your data subject rights, please contact our dedicated Data Protection Officer (DPO):
              </p>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <a
                  href="mailto:privacy@airsystem.com"
                  className="flex items-center gap-3 p-3.5 rounded-2xl bg-slate-800/80 hover:bg-brand-600 transition group"
                >
                  <Mail className="h-5 w-5 text-brand-400 group-hover:text-white" />
                  <div>
                    <p className="text-xs text-slate-400">Email Data Privacy Office</p>
                    <p className="text-sm font-semibold text-white">privacy@{companyName.toLowerCase().replace(/\s+/g, '')}.com</p>
                  </div>
                </a>
                <div className="flex items-center gap-3 p-3.5 rounded-2xl bg-slate-800/80">
                  <Phone className="h-5 w-5 text-brand-400" />
                  <div>
                    <p className="text-xs text-slate-400">24/7 Security Helpline</p>
                    <p className="text-sm font-semibold text-white">+1 (800) 555-PRIVACY</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </PageWrapper>
  );
}
