import React,{useEffect,useRef,useState}from'react';
import{createRoot}from'react-dom/client';
import'./style.css';

const API=import.meta.env.VITE_API_URL??'http://localhost:8080/api/v1';
const token=()=>localStorage.getItem('dado_token')??'';

async function req(path:string,method='GET',body?:any){
  const r=await fetch(API+path,{method,headers:{'Content-Type':'application/json',Authorization:`Bearer ${token()}`},body:body!==undefined?JSON.stringify(body):undefined});
  if(!r.ok){
    const raw=await r.text();
    let message=raw||`HTTP ${r.status}`;
    try{const parsed=JSON.parse(raw);message=parsed.message??raw}catch{}
    if(r.status===401)localStorage.removeItem('dado_token');
    throw new Error(message);
  }
  return r.status===204?null:r.json();
}

const paths:any={products:'/products',suppliers:'/suppliers',purchases:'/purchases',prices:'/prices',stocks:'/reports/stock',sales:'/sales',inventories:'/inventories',movements:'/reports/movements',differences:'/reports/inventory-differences',promotions:'/promotions'};
const titles:any={products:'Товары',suppliers:'Поставщики',purchases:'Приход товара',prices:'Цены',stocks:'Остатки',sales:'Продажи',inventories:'Инвентаризация',movements:'Движение товара',differences:'Расхождения',promotions:'Акции'};
const purchaseStatus:any={0:'Черновик',1:'Проведен',2:'Отменен',Draft:'Черновик',Posted:'Проведен',Cancelled:'Отменен'};
const saleStatus:any={0:'Черновик',1:'Ожидает оплату',2:'Оплачен',3:'Ожидает фискализацию',4:'Завершен',5:'Отменен',6:'Возврат',Draft:'Черновик',AwaitingPayment:'Ожидает оплату',Paid:'Оплачен',AwaitingFiscalization:'Ожидает фискализацию',Completed:'Завершен',Cancelled:'Отменен',Refunded:'Возврат'};
const money=(v:any)=>Number(v??0).toFixed(2);
const date=(v:any)=>v?new Date(v).toLocaleString('ru-RU'):'—';

function Login({onDone}:{onDone:()=>void}){
  const[username,setUsername]=useState('pilotadmin');const[password,setPassword]=useState('');const[msg,setMsg]=useState('');const[busy,setBusy]=useState(false);
  async function submit(e:any){e.preventDefault();setBusy(true);setMsg('');try{const r=await fetch(API+'/auth/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({username,password})});if(!r.ok)throw new Error('Неверный логин или пароль');const d=await r.json();localStorage.setItem('dado_token',d.accessToken);localStorage.setItem('dado_user',d.user?.username??username);onDone()}catch(e:any){setMsg(e.message)}finally{setBusy(false)}}
  return <div className="loginPage"><form className="loginCard" onSubmit={submit}><div className="brand">ДАДО</div><h2>Retail Admin</h2><p>Вход в панель управления магазином</p>{msg&&<div className="error">{msg}</div>}<label>Логин<input autoFocus required value={username} onChange={e=>setUsername(e.target.value)}/></label><label>Пароль<input required type="password" value={password} onChange={e=>setPassword(e.target.value)}/></label><button className="primary" disabled={busy}>{busy?'Вход...':'Войти'}</button></form></div>
}

function App(){
  const[authed,setAuthed]=useState(!!token());
  const[tab,setTab]=useState('products');const[data,setData]=useState<any[]>([]);const[dash,setDash]=useState<any>();const[msg,setMsg]=useState('');const[notice,setNotice]=useState('');
  const[show,setShow]=useState(false);const[form,setForm]=useState<any>({});const[selected,setSelected]=useState<any>();
  const[stores,setStores]=useState<any[]>([]);const[warehouses,setWarehouses]=useState<any[]>([]);const[suppliers,setSuppliers]=useState<any[]>([]);const[products,setProducts]=useState<any[]>([]);
  const[barcode,setBarcode]=useState('');const[qty,setQty]=useState('');const[purchasePrice,setPurchasePrice]=useState('');const[expiry,setExpiry]=useState('');
  const scan=useRef<HTMLInputElement>(null);

  const storeName=(id:string)=>stores.find(x=>x.id===id)?.name??id;
  const warehouseName=(id:string)=>warehouses.find(x=>x.id===id)?.name??id;
  const supplierName=(id:string)=>suppliers.find(x=>x.id===id)?.name??id;
  const productName=(id:string)=>products.find(x=>x.id===id)?.name??id;

  async function loadMasters(){
    const [ss,sp,pp]=await Promise.all([req('/organization/stores'),req('/suppliers'),req('/products')]);
    setStores(ss);setSuppliers(sp);setProducts(pp);
    const ww=(await Promise.all(ss.map((s:any)=>req(`/organization/warehouses?storeId=${s.id}`)))).flat();
    setWarehouses(ww);
  }

  async function load(t=tab){
    setTab(t);setSelected(undefined);setNotice('');
    try{const rows=await req(paths[t]);setData(Array.isArray(rows)?rows:[]);try{setDash(await req('/reports/dashboard'))}catch{}setMsg('')}catch(e:any){setMsg(e.message);if(!token())setAuthed(false)}
  }

  useEffect(()=>{if(authed){loadMasters().then(()=>load()).catch((e:any)=>{setMsg(e.message);if(!token())setAuthed(false)})}},[authed]);

  async function save(e:any){
    e.preventDefault();setMsg('');
    try{
      if(tab==='products')await req('/products','POST',{sku:form.sku,name:form.name,unitOfMeasure:form.unit||'шт',barcodes:(form.barcode||'').split(',').map((x:string)=>x.trim()).filter(Boolean)});
      if(tab==='suppliers')await req('/suppliers','POST',{name:form.name,taxId:form.taxId||null,phone:form.phone||null});
      if(tab==='prices')await req('/prices','POST',{productId:form.productId,storeId:form.storeId,amount:Number(form.amount),validFromUtc:form.validFromUtc?new Date(form.validFromUtc).toISOString():null,validToUtc:form.validToUtc?new Date(form.validToUtc).toISOString():null});
      if(tab==='purchases'){
        const d=await req('/purchases','POST',{supplierId:form.supplierId,warehouseId:form.warehouseId,documentNumber:form.documentNumber});
        setShow(false);setForm({});await loadMasters();await load('purchases');await openPurchase(d.id);setNotice('Документ прихода создан. Добавьте товар по штрихкоду.');return;
      }
      if(tab==='inventories'){
        const d=await req('/inventories','POST',{warehouseId:form.warehouseId,number:form.number});await req(`/inventories/${d.id}/start`,'POST');
        setShow(false);setForm({});await openInventory(d.id);await load('inventories');return;
      }
      setShow(false);setForm({});setNotice('Сохранено');await loadMasters();await load();
    }catch(e:any){setMsg(e.message)}
  }

  async function openPurchase(id:string){try{setSelected(await req(`/purchases/${id}`));setMsg('');setTimeout(()=>scan.current?.focus(),50)}catch(e:any){setMsg(e.message)}}
  async function addPurchaseItem(e:any){e.preventDefault();if(!selected||!barcode||!qty||!purchasePrice)return;try{await req(`/purchases/${selected.id}/items/by-barcode`,'POST',{barcode,quantity:Number(qty),purchasePrice:Number(purchasePrice),expiryDate:expiry||null});setBarcode('');setQty('');setPurchasePrice('');setExpiry('');await openPurchase(selected.id);setNotice('Позиция добавлена')}catch(e:any){setMsg(e.message)}}
  async function postPurchase(){if(!selected)return;try{await req(`/purchases/${selected.id}/post`,'POST');setNotice('Приход проведен. Остатки склада увеличены.');await openPurchase(selected.id);await load('purchases')}catch(e:any){setMsg(e.message)}}

  async function openInventory(id:string){try{setSelected(await req(`/inventories/${id}`));setMsg('');setTimeout(()=>scan.current?.focus(),50)}catch(e:any){setMsg(e.message)}}
  async function count(e:any){e.preventDefault();if(!selected||!barcode||qty==='')return;try{await req(`/inventories/${selected.id}/count`,'POST',{barcode,actualQuantity:Number(qty)});setBarcode('');setQty('');await openInventory(selected.id)}catch(e:any){setMsg(e.message)}}
  async function inventoryAction(name:string){if(!selected)return;try{await req(`/inventories/${selected.id}/${name}`,'POST');await openInventory(selected.id);await load('inventories')}catch(e:any){setMsg(e.message)}}

  function logout(){localStorage.removeItem('dado_token');localStorage.removeItem('dado_user');setAuthed(false)}
  if(!authed)return <Login onDone={()=>setAuthed(true)}/>;

  const menu=[['products','Товары'],['suppliers','Поставщики'],['purchases','Приход товара'],['stocks','Остатки'],['sales','Продажи'],['prices','Цены'],['inventories','Инвентаризация'],['movements','Движение товара'],['differences','Расхождения'],['promotions','Акции']];
  const canCreate=['products','suppliers','purchases','prices','inventories'].includes(tab);

  function table(){
    if(tab==='products')return <><thead><tr><th>SKU</th><th>Название</th><th>Ед.</th><th>Штрихкоды</th><th>Активен</th></tr></thead><tbody>{data.map(r=><tr key={r.id}><td>{r.sku}</td><td>{r.name}</td><td>{r.unitOfMeasure}</td><td>{(r.barcodes||[]).join(', ')}</td><td>{r.isActive?'Да':'Нет'}</td></tr>)}</tbody></>;
    if(tab==='suppliers')return <><thead><tr><th>Поставщик</th><th>ИНН</th><th>Телефон</th></tr></thead><tbody>{data.map(r=><tr key={r.id}><td>{r.name}</td><td>{r.taxId||'—'}</td><td>{r.phone||'—'}</td></tr>)}</tbody></>;
    if(tab==='purchases')return <><thead><tr><th>Документ</th><th>Поставщик</th><th>Склад</th><th>Статус</th><th>Позиций</th><th>Сумма</th><th>Дата</th></tr></thead><tbody>{data.map(r=><tr key={r.id} className="clickable" onClick={()=>openPurchase(r.id)}><td>{r.documentNumber}</td><td>{supplierName(r.supplierId)}</td><td>{warehouseName(r.warehouseId)}</td><td><span className="badge">{purchaseStatus[r.status]??r.status}</span></td><td>{r.items}</td><td>{money(r.total)}</td><td>{date(r.createdAtUtc)}</td></tr>)}</tbody></>;
    if(tab==='prices')return <><thead><tr><th>Товар</th><th>Магазин</th><th>Цена</th><th>Действует с</th><th>Действует до</th><th>Активна</th></tr></thead><tbody>{data.map(r=><tr key={r.id}><td>{productName(r.productId)}</td><td>{storeName(r.storeId)}</td><td>{money(r.amount)}</td><td>{date(r.validFromUtc)}</td><td>{date(r.validToUtc)}</td><td>{r.isActive?'Да':'Нет'}</td></tr>)}</tbody></>;
    if(tab==='stocks')return <><thead><tr><th>SKU</th><th>Товар</th><th>Склад</th><th>Остаток</th></tr></thead><tbody>{data.map((r,i)=><tr key={`${r.productId}-${r.warehouseId}-${i}`}><td>{r.sku}</td><td>{r.name}</td><td>{warehouseName(r.warehouseId)}</td><td className={Number(r.quantity)<=5?'low':''}>{r.quantity}</td></tr>)}</tbody></>;
    if(tab==='sales')return <><thead><tr><th>Дата</th><th>Магазин</th><th>Статус</th><th>Позиций</th><th>Сумма</th><th>Оплачено</th><th>ID чека</th></tr></thead><tbody>{data.map(r=><tr key={r.id}><td>{date(r.createdAtUtc)}</td><td>{storeName(r.storeId)}</td><td><span className="badge">{saleStatus[r.status]??r.status}</span></td><td>{r.items}</td><td>{money(r.total)}</td><td>{money(r.paid)}</td><td className="mono">{r.id.slice(0,8)}</td></tr>)}</tbody></>;
    return <><thead><tr>{data[0]&&Object.keys(data[0]).slice(0,9).map(k=><th key={k}>{k}</th>)}</tr></thead><tbody>{data.map((r,i)=><tr key={r.id??i} className={tab==='inventories'?'clickable':''} onClick={()=>tab==='inventories'&&openInventory(r.id)}>{Object.values(r).slice(0,9).map((v:any,j)=><td key={j}>{typeof v==='object'?JSON.stringify(v):String(v??'')}</td>)}</tr>)}</tbody></>;
  }

  return <div className="app"><aside><h1>ДАДО</h1><p>Retail Admin</p>{menu.map(x=><button key={x[0]} className={tab===x[0]?'active':''} onClick={()=>load(x[0])}>{x[1]}</button>)}<div className="asideFoot"><span>{localStorage.getItem('dado_user')||'Администратор'}</span><button onClick={logout}>Выйти</button></div></aside><main><header><div><h2>{titles[tab]}</h2><small>Централизованное управление магазином</small></div><div>{canCreate&&<button className="primary" onClick={()=>{setForm({});setShow(true)}}>+ Создать</button>} <button onClick={()=>load()}>Обновить</button></div></header>{msg&&<div className="error">{msg}</div>}{notice&&<div className="notice">{notice}</div>}{dash&&<section className="cards"><div><b>{money(dash.salesToday)}</b><span>Продажи сегодня, сомони</span></div><div><b>{dash.checksToday}</b><span>Чеков сегодня</span></div><div><b>{dash.stockUnits}</b><span>Единиц на остатке</span></div><div><b>{dash.products}</b><span>Товаров</span></div></section>}

  {show&&<form className="editor" onSubmit={save}><h3>Создать: {titles[tab]}</h3>
    {tab==='products'&&<><label>SKU<input required placeholder="Например DADO-001" onChange={e=>setForm({...form,sku:e.target.value})}/></label><label>Название<input required placeholder="Название товара" onChange={e=>setForm({...form,name:e.target.value})}/></label><label>Ед. измерения<input placeholder="шт" onChange={e=>setForm({...form,unit:e.target.value})}/></label><label>Штрихкод<input placeholder="Можно несколько через запятую" onChange={e=>setForm({...form,barcode:e.target.value})}/></label></>}
    {tab==='suppliers'&&<><label>Название<input required placeholder="Название поставщика" onChange={e=>setForm({...form,name:e.target.value})}/></label><label>ИНН<input placeholder="Необязательно" onChange={e=>setForm({...form,taxId:e.target.value})}/></label><label>Телефон<input placeholder="Необязательно" onChange={e=>setForm({...form,phone:e.target.value})}/></label></>}
    {tab==='prices'&&<><label>Товар<select required value={form.productId||''} onChange={e=>setForm({...form,productId:e.target.value})}><option value="">Выберите товар</option>{products.map(p=><option key={p.id} value={p.id}>{p.sku} — {p.name}</option>)}</select></label><label>Магазин<select required value={form.storeId||''} onChange={e=>setForm({...form,storeId:e.target.value})}><option value="">Выберите магазин</option>{stores.map(s=><option key={s.id} value={s.id}>{s.code} — {s.name}</option>)}</select></label><label>Цена<input required type="number" min="0" step="0.01" onChange={e=>setForm({...form,amount:e.target.value})}/></label><label>Действует с<input type="datetime-local" onChange={e=>setForm({...form,validFromUtc:e.target.value})}/></label><label>Действует до<input type="datetime-local" onChange={e=>setForm({...form,validToUtc:e.target.value})}/></label></>}
    {tab==='purchases'&&<><label>Поставщик<select required value={form.supplierId||''} onChange={e=>setForm({...form,supplierId:e.target.value})}><option value="">Выберите поставщика</option>{suppliers.map(s=><option key={s.id} value={s.id}>{s.name}</option>)}</select></label><label>Склад<select required value={form.warehouseId||''} onChange={e=>setForm({...form,warehouseId:e.target.value})}><option value="">Выберите склад</option>{warehouses.map(w=><option key={w.id} value={w.id}>{storeName(w.storeId)} / {w.name}</option>)}</select></label><label>Номер накладной<input required placeholder="Например PR-2026-001" onChange={e=>setForm({...form,documentNumber:e.target.value})}/></label></>}
    {tab==='inventories'&&<><label>Склад<select required value={form.warehouseId||''} onChange={e=>setForm({...form,warehouseId:e.target.value})}><option value="">Выберите склад</option>{warehouses.map(w=><option key={w.id} value={w.id}>{storeName(w.storeId)} / {w.name}</option>)}</select></label><label>Номер инвентаризации<input required placeholder="INV-2026-001" onChange={e=>setForm({...form,number:e.target.value})}/></label></>}
    <div><button className="primary">Сохранить</button><button type="button" onClick={()=>setShow(false)}>Отмена</button></div></form>}

  {tab==='purchases'&&selected&&<section className="inventory"><div className="inventoryHead"><div><h3>Приход {selected.documentNumber}</h3><span>{supplierName(selected.supplierId)} · {warehouseName(selected.warehouseId)} · {purchaseStatus[selected.status]??selected.status}</span></div><button onClick={()=>setSelected(undefined)}>Закрыть</button></div>{(selected.status===0||selected.status==='Draft')&&<form className="scanner purchaseScanner" onSubmit={addPurchaseItem}><input ref={scan} autoFocus required value={barcode} placeholder="Сканируйте штрихкод" onChange={e=>setBarcode(e.target.value)}/><input required type="number" step="0.001" min="0.001" value={qty} placeholder="Количество" onChange={e=>setQty(e.target.value)}/><input required type="number" step="0.01" min="0" value={purchasePrice} placeholder="Закупочная цена" onChange={e=>setPurchasePrice(e.target.value)}/><input type="date" value={expiry} onChange={e=>setExpiry(e.target.value)}/><button className="primary">Добавить</button></form>}<div className="summary"><b>Позиций: {selected.items?.length||0}</b><b>Сумма: {money(selected.total)}</b></div><div className="table"><table><thead><tr><th>SKU</th><th>Товар</th><th>Количество</th><th>Цена</th><th>Сумма</th><th>Срок годности</th></tr></thead><tbody>{(selected.items||[]).map((l:any)=><tr key={l.id}><td>{l.sku}</td><td>{l.name}</td><td>{l.quantity}</td><td>{money(l.purchasePrice)}</td><td>{money(l.total)}</td><td>{l.expiryDate||'—'}</td></tr>)}</tbody></table></div>{(selected.status===0||selected.status==='Draft')&&<div className="actions"><button className="primary" disabled={!selected.items?.length} onClick={postPurchase}>Провести приход и увеличить остатки</button></div>}</section>}

  {tab==='inventories'&&selected&&<section className="inventory"><div className="inventoryHead"><div><h3>Инвентаризация {selected.number}</h3><span>Статус: {selected.status} · Склад: {warehouseName(selected.warehouseId)}</span></div><button onClick={()=>setSelected(undefined)}>Закрыть</button></div>{selected.status==='Counting'&&<form className="scanner" onSubmit={count}><input ref={scan} autoFocus value={barcode} placeholder="Сканируйте штрихкод" onChange={e=>setBarcode(e.target.value)}/><input type="number" step="0.001" min="0" value={qty} placeholder="Фактическое количество" onChange={e=>setQty(e.target.value)}/><button className="primary">Записать</button></form>}<div className="summary"><b>Позиций: {selected.lines?.length||0}</b><b>Общее расхождение: {Number(selected.difference||0).toFixed(3)}</b></div><div className="table"><table><thead><tr><th>SKU</th><th>Товар</th><th>Учет</th><th>Факт</th><th>Разница</th></tr></thead><tbody>{(selected.lines||[]).map((l:any)=><tr key={l.productId} className={l.difference!==0?'diff':''}><td>{l.sku}</td><td>{l.name}</td><td>{l.bookQuantity}</td><td>{l.actualQuantity}</td><td>{l.difference>0?'+':''}{l.difference}</td></tr>)}</tbody></table></div><div className="actions">{selected.status==='Counting'&&<><button className="primary" onClick={()=>inventoryAction('submit')}>Отправить на утверждение</button><button onClick={()=>inventoryAction('cancel')}>Отменить документ</button></>}{selected.status==='AwaitingApproval'&&<><button className="primary" onClick={()=>inventoryAction('approve')}>Утвердить и скорректировать остатки</button><button onClick={()=>inventoryAction('cancel')}>Отменить документ</button></>}</div></section>}

  <div className="table"><table>{table()}</table>{!data.length&&!msg&&<p className="empty">Нет данных</p>}</div></main></div>
}

createRoot(document.getElementById('root')!).render(<App/>);
