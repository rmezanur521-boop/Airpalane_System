import { useCallback, useEffect, useState } from 'react';
import { Search, Trash2, UserCheck, UserX, Plus } from 'lucide-react';
import Table       from '@/components/ui/Table';
import Pagination  from '@/components/ui/Pagination';
import Badge       from '@/components/ui/Badge';
import Button      from '@/components/ui/Button';
import Modal       from '@/components/ui/Modal';
import Input       from '@/components/ui/Input';
import Alert       from '@/components/ui/Alert';
import userService  from '@/api/userService';
import adminService from '@/api/adminService';
import { usePagination } from '@/hooks/usePagination';
import { useDebounce }   from '@/hooks/useDebounce';
import { formatDate, formatDateTime } from '@/utils/formatters';
import toast from 'react-hot-toast';

const AGENT_EMPTY = { firstName: '', lastName: '', email: '', phoneNumber: '' };

export default function UsersAdminPage() {
  const [users,   setUsers]   = useState([]);
  const [total,   setTotal]   = useState(1);
  const [loading, setLoading] = useState(true);
  const [search,  setSearch]  = useState('');
  const debSearch             = useDebounce(search);
  const { pageNumber, pageSize, goToPage, resetPage } = usePagination(15);

  // Delete modal
  const [deleteModal,  setDeleteModal]  = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting,     setDeleting]     = useState(false);

  // Create agent modal
  const [agentModal,  setAgentModal]  = useState(false);
  const [agentForm,   setAgentForm]   = useState(AGENT_EMPTY);
  const [agentSaving, setAgentSaving] = useState(false);
  const [agentError,  setAgentError]  = useState('');

  useEffect(() => { resetPage(); }, [debSearch]);

  const load = useCallback(() => {
    setLoading(true);
    userService
      .getAllUsers({ pageNumber, pageSize, searchTerm: debSearch })
      .then(({ data }) => {
        setUsers(data.items ?? []);
        setTotal(data.totalPages ?? 1);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [pageNumber, pageSize, debSearch]);

  useEffect(load, [load]);

  const handleToggleActive = async (user) => {
    try {
      await userService.setUserActive(user.id, !user.isActive);
      toast.success(`User ${user.isActive ? 'deactivated' : 'activated'}.`);
      load();
    } catch {
      toast.error('Failed to update user status.');
    }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await userService.deleteUser(deleteTarget.id);
      toast.success('User deleted.');
      setDeleteModal(false);
      load();
    } catch {
      toast.error('Delete failed.');
    } finally {
      setDeleting(false);
    }
  };

  const handleCreateAgent = async () => {
    setAgentError('');
    setAgentSaving(true);
    try {
      await adminService.createAgent(agentForm);
      toast.success('Agent account created.');
      setAgentModal(false);
      setAgentForm(AGENT_EMPTY);
      load();
    } catch (err) {
      setAgentError(err.response?.data?.detail ?? 'Failed to create agent.');
    } finally {
      setAgentSaving(false);
    }
  };

  const roleColor = { Admin: 'red', Agent: 'brand', Passenger: 'slate' };

  const columns = [
    {
      key: 'fullName',
      header: 'Name',
      render: (u) => (
        <div>
          <p className="font-medium text-slate-800">{u.fullName}</p>
          <p className="text-xs text-slate-400">{u.email}</p>
        </div>
      ),
    },
    {
      key: 'role',
      header: 'Role',
      render: (u) => (
        <Badge color={roleColor[u.role] ?? 'slate'}>{u.role}</Badge>
      ),
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (u) => (
        <Badge color={u.isActive ? 'green' : 'red'}>
          {u.isActive ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      key: 'isEmailVerified',
      header: 'Email',
      render: (u) => (
        <Badge color={u.isEmailVerified ? 'green' : 'yellow'}>
          {u.isEmailVerified ? 'Verified' : 'Pending'}
        </Badge>
      ),
    },
    {
      key: 'createdAt',
      header: 'Joined',
      render: (u) => formatDate(u.createdAt),
    },
    {
      key: 'actions',
      header: '',
      render: (u) => (
        <div className="flex items-center gap-1">
          <button
            onClick={() => handleToggleActive(u)}
            className={`p-1.5 rounded-lg transition ${
              u.isActive
                ? 'text-slate-400 hover:text-red-600 hover:bg-red-50'
                : 'text-slate-400 hover:text-green-600 hover:bg-green-50'
            }`}
            title={u.isActive ? 'Deactivate' : 'Activate'}
          >
            {u.isActive
              ? <UserX className="h-4 w-4" />
              : <UserCheck className="h-4 w-4" />}
          </button>
          <button
            onClick={() => { setDeleteTarget(u); setDeleteModal(true); }}
            className="p-1.5 rounded-lg text-slate-400 hover:text-red-600 hover:bg-red-50 transition"
          >
            <Trash2 className="h-4 w-4" />
          </button>
        </div>
      ),
    },
  ];

  return (
    <div className="animate-fadeIn">
      <div className="flex items-center justify-between mb-6 flex-wrap gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Users</h1>
          <p className="text-slate-500 text-sm mt-1">Manage all user accounts</p>
        </div>
        <div className="flex items-center gap-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              className="input-base pl-9 w-56"
              placeholder="Search users…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <Button onClick={() => { setAgentForm(AGENT_EMPTY); setAgentError(''); setAgentModal(true); }}>
            <Plus className="h-4 w-4" /> Add Agent
          </Button>
        </div>
      </div>

      <Table columns={columns} data={users} loading={loading} />
      <Pagination pageNumber={pageNumber} totalPages={total} onPage={goToPage} />

      {/* Delete Modal */}
      <Modal open={deleteModal} onClose={() => setDeleteModal(false)} title="Delete User">
        <Alert type="error"
          message={`Delete ${deleteTarget?.fullName}? This cannot be undone.`} />
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="secondary" onClick={() => setDeleteModal(false)}>Cancel</Button>
          <Button variant="danger" loading={deleting} onClick={handleDelete}>Delete</Button>
        </div>
      </Modal>

      {/* Create Agent Modal */}
      <Modal open={agentModal} onClose={() => setAgentModal(false)} title="Create Agent Account">
        {agentError && <Alert type="error" message={agentError} className="mb-4" />}
        <div className="grid grid-cols-2 gap-4">
          <Input label="First name" value={agentForm.firstName}
            onChange={(e) => setAgentForm((p) => ({ ...p, firstName: e.target.value }))}
            required />
          <Input label="Last name" value={agentForm.lastName}
            onChange={(e) => setAgentForm((p) => ({ ...p, lastName: e.target.value }))}
            required />
          <Input label="Email" type="email" value={agentForm.email}
            onChange={(e) => setAgentForm((p) => ({ ...p, email: e.target.value }))}
            required className="col-span-2" containerClassName="col-span-2" />
          <Input label="Phone" value={agentForm.phoneNumber}
            onChange={(e) => setAgentForm((p) => ({ ...p, phoneNumber: e.target.value }))}
            className="col-span-2" containerClassName="col-span-2" />
        </div>
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="secondary" onClick={() => setAgentModal(false)}>Cancel</Button>
          <Button loading={agentSaving} onClick={handleCreateAgent}>Create Agent</Button>
        </div>
      </Modal>
    </div>
  );
}