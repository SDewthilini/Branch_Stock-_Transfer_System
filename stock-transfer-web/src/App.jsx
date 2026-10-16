import React, { useEffect, useState } from 'react';
import API from './api/axiosInstance';
import AvailableStockViewer from './components/AvailableStockViewer';
import NewTransferRequest from './components/NewTransferRequest';
import TransferListManagement from './components/TransferListManagement';
import './App.css';

// Loads shared data and coordinates stock and transfer actions across the main view.
function App() {
  const [branches, setBranches] = useState([]);
  const [products, setProducts] = useState([]);
  const [selectedBranch, setSelectedBranch] = useState('');
  const [stocks, setStocks] = useState([]);
  const [fromBranchId, setFromBranchId] = useState('');
  const [toBranchId, setToBranchId] = useState('');
  const [productId, setProductId] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [errorMsg, setErrorMsg] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [transfers, setTransfers] = useState([]);

  const loadInitialData = async () => {
    try {
      const branchRes = await API.get('/branches');
      setBranches(branchRes.data);

      const transferRes = await API.get('/transfers');
      setTransfers(transferRes.data);

      const productRes = await API.get('/products');
      setProducts(productRes.data);
    } catch (err) {
      console.error('Error loading initial data', err);
    }
  };

  useEffect(() => {
    loadInitialData();
  }, []);

  const fetchStock = async (branchId) => {
    setSelectedBranch(branchId);
    if (!branchId) {
      setStocks([]);
      return;
    }
    try {
      const res = await API.get(`/branches/${branchId}/stock`);
      setStocks(res.data);
    } catch (err) {
      console.error('Error fetching stock', err);
      setStocks([]);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrorMsg('');
    setSuccessMsg('');

    if (fromBranchId === toBranchId) {
      setErrorMsg('From Branch and To Branch cannot be the same.');
      return;
    }

    if (!productId) {
      setErrorMsg('Please select a product.');
      return;
    }

    try {
      await API.post('/transfers', {
        fromBranchId: parseInt(fromBranchId),
        toBranchId: parseInt(toBranchId),
        items: [{ productId: parseInt(productId), quantity: parseInt(quantity) }]
      });
      setSuccessMsg('Transfer Request Submitted Successfully!');
      loadInitialData();
    } catch (err) {
      setErrorMsg(err.response?.data || 'Error creating transfer request');
    }
  };

  const handleAction = async (id, actionType) => {
    try {
      setErrorMsg('');
      setSuccessMsg('');
      await API.post(`/transfers/${id}/${actionType}`);

      setSuccessMsg(`Transfer successfully marked as ${actionType}ed!`);
      loadInitialData();
    } catch (err) {
      setErrorMsg(err.response?.data || `Error performing ${actionType} action.`);
    }
  };

  const getBranchName = (id) => {
    const branch = branches.find(b => b.id === parseInt(id));
    return branch ? branch.name : `Branch #${id}`;
  };

  const formatTransferId = (id) => `TR-${String(id).padStart(3, '0')}`;

  const getStatusBadge = (status) => {
    const statusText = typeof status === 'number'
      ? (status === 0 ? 'Requested' : status === 1 ? 'Dispatched' : status === 2 ? 'Received' : 'Cancelled')
      : status;

    switch (statusText) {
      case 'Requested':
        return <span className="px-3 py-1 text-xs font-semibold rounded-full bg-amber-100 text-amber-800">Requested</span>;
      case 'Dispatched':
        return <span className="px-3 py-1 text-xs font-semibold rounded-full bg-blue-100 text-blue-800">Dispatched</span>;
      case 'Received':
        return <span className="px-3 py-1 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800">Received</span>;
      case 'Cancelled':
        return <span className="px-3 py-1 text-xs font-semibold rounded-full bg-rose-100 text-rose-800">Cancelled</span>;
      default:
        return <span className="px-3 py-1 text-xs font-semibold rounded-full bg-slate-100 text-slate-800">{statusText}</span>;
    }
  };

  const getStatusString = (status) => {
    if (typeof status === 'number') {
      return status === 0 ? 'Requested' : status === 1 ? 'Dispatched' : status === 2 ? 'Received' : 'Cancelled';
    }
    return status;
  };

  return (
    <main className="dashboard-shell">
      <div className="dashboard-container">
        <header className="dashboard-header">
          <div className="dashboard-header-copy">
            <p className="eyebrow">Operations dashboard</p>
            <h1 className="dashboard-title">Branch Stock Transfer</h1>
            <p className="dashboard-subtitle">Manage branch inventory and keep every transfer moving.</p>
          </div>
          <span className="system-status">System active</span>
        </header>

        {errorMsg && (
          <div className="dashboard-alert dashboard-alert-error" role="alert">
            <span>{errorMsg}</span>
            <button onClick={() => setErrorMsg('')} aria-label="Dismiss error">&times;</button>
          </div>
        )}
        {successMsg && (
          <div className="dashboard-alert dashboard-alert-success" role="status">
            <span>{successMsg}</span>
            <button onClick={() => setSuccessMsg('')} aria-label="Dismiss success message">&times;</button>
          </div>
        )}

        <section className="dashboard-grid" aria-label="Inventory tools">
          <AvailableStockViewer
            branches={branches}
            selectedBranch={selectedBranch}
            stocks={stocks}
            onBranchChange={fetchStock}
          />

          <NewTransferRequest
            branches={branches}
            products={products}
            fromBranchId={fromBranchId}
            toBranchId={toBranchId}
            productId={productId}
            quantity={quantity}
            onFromBranchChange={setFromBranchId}
            onToBranchChange={setToBranchId}
            onProductChange={setProductId}
            onQuantityChange={setQuantity}
            onSubmit={handleSubmit}
          />
        </section>

        <TransferListManagement
          transfers={transfers}
          onRefresh={loadInitialData}
          getBranchName={getBranchName}
          formatTransferId={formatTransferId}
          getStatusBadge={getStatusBadge}
          getStatusString={getStatusString}
          onAction={handleAction}
        />
      </div>
    </main>
  );
}

export default App;
