import { useCallback, useEffect, useState } from 'react';
import { Search, ScrollText } from 'lucide-react';
import Table        from '@/components/ui/Table';
import Pagination   from '@/components/ui/Pagination';
import Input        from '@/components/ui/Input';
import Select       from '@/components/ui/Select';
import adminService from '@/api/adminService';
import { usePagination } from '@/hooks/usePagination';
import { useDebounce }   from '@/hooks/useDebounce';
import { formatDateTime } from '@/utils/formatters';

const ACTION_OPTIONS = [
  { value: '',       label: 'All actions' },
  { value: 'Create', label: 'Create' },
  { value: 'Update', label: 'Update' },
  { value: 'Delete', label: 'Delete' },
  { value: 'Login',  label: 'Login' },
];

export default function AuditLogsPage() {
  const [logs,    setLogs]    = useState([]);
  const [total,   setTotal]   = useState(1);
  const [loading, setLoading] = useState(true);
  const [search,  setSearch]  = useState('');
  const [action,  setAction]  = useState('');
  const [entity,  setEntity]  = useState('');
  const debSearch             = useDebounce(search);
  const debEntity             = useDebounce(entity);
  const { pageNumber, pageSize, goToPage, resetPage } = usePagination(20);

  useEffect(() => { resetPage(); }, [debSearch, action, debEntity]);

  const load = useCallback(() => {
    setLoading(true);
    adminService
      .getAuditLogs({
        PageNumber:  pageNumber,
        PageSize:    pageSize,
        SearchTerm:  debSearch,
        Action:      action || undefined,
        EntityName:  debEntity || undefined,
        SortBy:      'timestamp',
        SortDescending: true,
      })
      .then(({ data }) => {
        setLogs(data.items ?? []);
        setTotal(data.totalPages ?? 1);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [pageNumber, pageSize, debSearch, action, debEntity]);

  useEffect(load, [load]);

  const actionColor = {
    Create: 'bg-green-100 text-green-700',
    Update: 'bg-blue-100 text-blue-700',
    Delete: 'bg-red-100 text-red-700',
    Login:  'bg-purple-100 text-purple-700',
  };

  const columns = [
    {
      key: 'timestamp',
      header: 'Time',
      render: (l) => <span className="text-xs">{formatDateTime(l.timestamp)}</span>,
    },
    { key: 'entityName', header: 'Entity' },
    { key: 'entityId',   header: 'Entity ID',
      render: (l) => (
        <span className="text-xs font-mono text-slate-500 truncate max-w-[120px] block">
          {l.entityId}
        </span>
      ),
    },
    {
      key: 'action',
      header: 'Action',
      render: (l) => (
        <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium
          ${actionColor[l.action] ?? 'bg-slate-100 text-slate-700'}`}>
          {l.action}
        </span>
      ),
    },
    { key: 'userEmail',  header: 'User' },
    { key: 'ipAddress',  header: 'IP Address' },
    {
      key: 'correlationId',
      header: 'Correlation ID',
      render: (l) => (
        <span className="text-xs font-mono text-slate-400">{l.correlationId ?? '—'}</span>
      ),
    },
  ];

  return (
    <div className="animate-fadeIn">
      {/* Header */}
      <div className="flex items-center gap-3 mb-6">
        <ScrollText className="h-6 w-6 text-brand-600" />
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Audit Logs</h1>
          <p className="text-slate-500 text-sm mt-0.5">System activity trail</p>
        </div>
      </div>

      {/* Filters */}
      <div className="card mb-6">
        <div className="flex flex-wrap gap-4">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              className="input-base pl-9"
              placeholder="Search logs…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <Input
            placeholder="Filter by entity (e.g. Flight)"
            value={entity}
            onChange={(e) => setEntity(e.target.value)}
            containerClassName="flex-1 min-w-[160px]"
          />
          <Select
            options={ACTION_OPTIONS}
            value={action}
            onChange={(e) => setAction(e.target.value)}
            containerClassName="w-44"
          />
        </div>
      </div>

      <Table columns={columns} data={logs} loading={loading}
        emptyMessage="No audit logs found." />
      <Pagination pageNumber={pageNumber} totalPages={total} onPage={goToPage} />
    </div>
  );
}