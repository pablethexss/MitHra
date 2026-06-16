import { Component, OnInit, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ProductosService, Producto } from '../../services/producto';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-productos',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './productos.html',
  styleUrls: ['./productos.css']
})
export class ProductosComponent implements OnInit {
  // 🔌 Inyecciones modernas
  private fb = inject(FormBuilder);
  public productosService = inject(ProductosService);

  // 📝 Variables de formulario y catálogos
  public productoForm!: FormGroup;
  public tiposProducto = ['Fisico', 'Servicio', 'Perecible'];

  // 🧠 OPTIMIZACIÓN DE TEMA: El tema vive en un Signal y calcula estados derivados
  public temaActual = signal(document.documentElement.getAttribute('data-theme') || 'industrial');
  public esDark = computed(() => this.temaActual() === 'industrial' || this.temaActual() === 'cyberpunk');
  public esCyberpunk = computed(() => this.temaActual() === 'cyberpunk');

  constructor() {
    // 🚦 REACTIVIDAD ANGULAR 22: Usamos effect para reaccionar al cambio del producto a editar
    effect(() => {
      const p = this.productosService.productoAEditar();
      if (p) {
        this.productoForm.patchValue(p);
      } else {
        // Reseteamos al estado inicial si no hay producto (ej: crear nuevo)
        this.productoForm.reset({ 
          codigoBarras: '',
          tipo: 'Fisico', 
          stock: 0, 
          stockMinimo: 0, 
          esAfectoIVA: true, 
          requiereMayorEdad: false 
        });
      }
    });
  }

  ngOnInit(): void {
    this.initForm();
    
    // Rastrea si el tema cambia dinámicamente
    const observer = new MutationObserver(() => {
      this.temaActual.set(document.documentElement.getAttribute('data-theme') || 'industrial');
    });
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });
  }

  // 🛠️ Inicialización del Formulario y Reglas de Negocio Reactivas
  private initForm(): void {
    this.productoForm = this.fb.group({
      id: [null], // <--- ESTO ES LO QUE FALTA
      codigo: ['', [Validators.required, Validators.minLength(3)]],
      codigoBarras: [''],
      nombre: ['', Validators.required],
      tipo: ['Fisico', Validators.required],
      stock: [0, [Validators.required, Validators.min(0)]],
      stockMinimo: [0, [Validators.required, Validators.min(0)]],
      precioNeto: [0, [Validators.required, Validators.min(1)]],
      esAfectoIVA: [true],
      requiereMayorEdad: [false],
      fechaCaducidad: [null]
    });

    this.productoForm.get('tipo')?.valueChanges.subscribe(tipo => {
      this.evaluarReglasPorTipo(tipo);
    });
  }

  private evaluarReglasPorTipo(tipo: string): void {
    const fechaCtrl = this.productoForm.get('fechaCaducidad');
    const stockCtrl = this.productoForm.get('stock');
    const stockMinCtrl = this.productoForm.get('stockMinimo');

    if (tipo === 'Perecible') {
      fechaCtrl?.setValidators([Validators.required]);
    } else {
      fechaCtrl?.clearValidators();
      fechaCtrl?.setValue(null);
    }

    if (tipo === 'Servicio') {
      stockCtrl?.setValue(0);
      stockMinCtrl?.setValue(0);
    }
    fechaCtrl?.updateValueAndValidity();
  }

  public async guardar(): Promise<void> {
    if (this.productoForm.invalid) {
      this.productoForm.markAllAsTouched();
      return;
    }
    // Combinamos los datos del formulario con el ID (si existe) que viene del servicio
    const productoAEnviar: Producto = { 
      ...this.productoForm.value, 
      id: this.productosService.productoAEditar()?.id 
    };
    try {
      await this.productosService.guardarProducto(productoAEnviar);
      alert('¡Producto guardado exitosamente!');
      this.productosService.cerrarModal();
      // Opcional: Refrescar la lista inmediatamente para ver el cambio
      this.productosService.cargarProductos();
    } catch (err: any) {
      if (err.status === 409) { // 409 Conflict
        alert('Error: Este código ya existe. Por favor usa uno diferente.');
      } else {
        alert('Hubo un problema al conectar con el servidor.');
      }
    }
  }
  public async eliminarProducto(id: number): Promise<void> {
    if (confirm('¿Estás seguro de que deseas eliminar este producto?')) {
        await this.productosService.eliminarProducto(id);
        this.productosService.listaProductos.update(lista => 
        lista.filter(p => p.id !== id)
      );
    }
  }
  async onFileSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e: any) => {
    const data = new Uint8Array(e.target.result);
    const workbook = XLSX.read(data, { type: 'array' });
    const worksheet = workbook.Sheets[workbook.SheetNames[0]];
    
    // Convertimos a JSON. 'header: 1' ayuda si quieres controlar los nombres de columnas
    const jsonData: any[] = XLSX.utils.sheet_to_json(worksheet);
    
    // Aquí jsonData contiene los objetos listos para guardar.
    // Debes validarlos antes de enviarlos.
    console.log("Datos cargados:", jsonData);
    this.enviarCargaMasiva(jsonData);
  };
  reader.readAsArrayBuffer(file);
}

async enviarCargaMasiva(productos: Producto[]) {
  try {
    // Necesitaremos crear este método en tu ProductosService
      await this.productosService.cargarMasiva(productos);
      alert('¡Carga masiva exitosa!');
      this.productosService.cargarProductos(); // Refrescar la lista
    } catch (error) {
      console.error(error);
      alert('Error al procesar el archivo.');
    }
  }
}