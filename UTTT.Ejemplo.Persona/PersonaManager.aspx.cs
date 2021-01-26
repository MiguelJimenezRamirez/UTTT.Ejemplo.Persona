#region Using

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UTTT.Ejemplo.Linq.Data.Entity;
using System.Data.Linq;
using System.Linq.Expressions;
using System.Collections;
using UTTT.Ejemplo.Persona.Control;
using UTTT.Ejemplo.Persona.Control.Ctrl;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Net;
#endregion

namespace UTTT.Ejemplo.Persona
{
    public partial class PersonaManager : System.Web.UI.Page
    {
        #region Variables

        private SessionManager session = new SessionManager();
        private int idPersona = 0;
        private UTTT.Ejemplo.Linq.Data.Entity.Persona baseEntity;
        private DataContext dcGlobal = new DcGeneralDataContext();
        private int tipoAccion = 0;

        #endregion

        #region Eventos

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this.Response.Buffer = true;
                this.session = (SessionManager)this.Session["SessionManager"];
                this.idPersona = this.session.Parametros["idPersona"] != null ?
                    int.Parse(this.session.Parametros["idPersona"].ToString()) : 0;
                if (this.idPersona == 0)
                {
                    this.baseEntity = new Linq.Data.Entity.Persona();
                    this.tipoAccion = 1;
                }
                else
                {
                    this.baseEntity = dcGlobal.GetTable<Linq.Data.Entity.Persona>().First(c => c.id == this.idPersona);
                    this.tipoAccion = 2;
                }

                if (!this.IsPostBack)
                {
                    if (this.session.Parametros["baseEntity"] == null)
                    {
                        this.session.Parametros.Add("baseEntity", this.baseEntity);
                    }
                    List<CatSexo> lista = dcGlobal.GetTable<CatSexo>().ToList();
                    CatSexo catTemp = new CatSexo();
                    catTemp.id = -1;
                    catTemp.strValor = "Seleccionar";
                    lista.Insert(0, catTemp);
                    this.ddlSexo.DataTextField = "strValor";
                    this.ddlSexo.DataValueField = "id";
                    this.ddlSexo.DataSource = lista;
                    this.ddlSexo.DataBind();

                    this.ddlSexo.SelectedIndexChanged += new EventHandler(ddlSexo_SelectedIndexChanged);
                    this.ddlSexo.AutoPostBack = true;
                    if (this.idPersona == 0)
                    {
                        lblAccion.Text = "Agregar";
                        DateTime tiempo = new DateTime(DateTime.Now.Year, DateTime.Now.Month,DateTime.Now.Day);
                        this.dteCalendar.TodaysDate = tiempo;
                        this.dteCalendar.SelectedDate = tiempo;
                        txtDia.Text = this.dteCalendar.SelectedDate.Day.ToString();
                        txtMes.Text = this.dteCalendar.SelectedDate.Month.ToString();
                        txtAnio.Text = this.dteCalendar.SelectedDate.Year.ToString();
                    }
                    else
                    {
                        this.lblAccion.Text = "Editar";
                        this.txtNombre.Text = this.baseEntity.strNombre;
                        this.txtAPaterno.Text = this.baseEntity.strAPaterno;
                        this.txtAMaterno.Text = this.baseEntity.strAMaterno;
                        this.txtClaveUnica.Text = this.baseEntity.strClaveUnica;
                        this.setItem(ref this.ddlSexo, baseEntity.CatSexo.strValor);
                        DateTime? fechaNacimiento = this.baseEntity.dtFechaDNaci;
                        this.txtNumeroHermanos.Text = this.baseEntity.intNumHermano.ToString();
                        if (fechaNacimiento != null) {
                            this.dteCalendar.TodaysDate = (DateTime)fechaNacimiento;
                            this.dteCalendar.SelectedDate = (DateTime)fechaNacimiento;
                        }
                        txtDia.Text = this.dteCalendar.SelectedDate.Day.ToString();
                        txtMes.Text = this.dteCalendar.SelectedDate.Month.ToString();
                        txtAnio.Text = this.dteCalendar.SelectedDate.Year.ToString();
                        this.txtCorreoElectronico.Text = this.baseEntity.strCorreoElectronico;
                        this.txtCodigoPostal.Text = this.baseEntity.intCodigoPostal.ToString();
                        this.txtRFC.Text = this.baseEntity.strRFC;

                        //Funcionalidad para numeros de hermanos
                       
                    }                
                }

            }
            catch (Exception _e)
            {
                this.showMessage("Ha ocurrido un problema al cargar la página");
                this.Response.Redirect("~/PersonaPrincipal.aspx", false);
                error(_e.ToString());
            }

        }

        protected void btnAceptar_Click(object sender, EventArgs e)
        {
            try
            {
                DataContext dcGuardar = new DcGeneralDataContext();
                UTTT.Ejemplo.Linq.Data.Entity.Persona persona = new Linq.Data.Entity.Persona();
                if (this.idPersona == 0)
                {
                    //agrega
                    persona.strClaveUnica = this.txtClaveUnica.Text.Trim();
                    persona.strNombre = this.txtNombre.Text.Trim();
                    persona.strAMaterno = this.txtAMaterno.Text.Trim();
                    persona.strAPaterno = this.txtAPaterno.Text.Trim();
                    persona.idCatSexo = int.Parse(this.ddlSexo.Text);
                    //El calendar
                    //DateTime fechaNacimiento = this.dteCalendar.SelectedDate.Date;
                    //persona.dtFechaDNaci = fechaNacimiento;

                    //string dateString = ;
                    //string format = "dd/MM/yyyy";
                    //DateTime dateTime = DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);

                    DateTime dateTime = Convert.ToDateTime(txtMes.Text + "/" + txtDia.Text + "/" + txtAnio.Text, CultureInfo.InvariantCulture);
                    persona.dtFechaDNaci = dateTime;


                    //agregar hermanos
                    persona.intNumHermano = int.Parse(this.txtNumeroHermanos.Text);
                    //Correo Electronico
                    persona.strCorreoElectronico = this.txtCorreoElectronico.Text.Trim();
                    //Codigo postal
                    persona.intCodigoPostal = int.Parse(this.txtCodigoPostal.Text);
                    //RFC
                    persona.strRFC = this.txtRFC.Text.Trim();
                    //Funcionalidad para hermanos
                    persona.intNumHermano = !this.txtNumeroHermanos.Text.Equals(string.Empty) ?
                        int.Parse(this.txtNumeroHermanos.Text) : 0;
                    String mensaje = string.Empty;
                    if (!this.validacion(persona, ref mensaje, txtDia.Text,txtMes.Text,txtAnio.Text)) {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;

                    }
                    if (!this.validacionSQL( ref mensaje))
                    {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;

                    }
                    if (!this.validacionHTML( ref mensaje))
                    {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;

                    }

                    dcGuardar.GetTable<UTTT.Ejemplo.Linq.Data.Entity.Persona>().InsertOnSubmit(persona);
                    dcGuardar.SubmitChanges();
                    this.showMessage("El registro se agrego correctamente.");
                    this.Response.Redirect("~/PersonaPrincipal.aspx", false);
                    
                }
                if (this.idPersona > 0)
                {
                    //edita ou<
                    persona = dcGuardar.GetTable<UTTT.Ejemplo.Linq.Data.Entity.Persona>().First(c => c.id == idPersona);
                    persona.strClaveUnica = this.txtClaveUnica.Text.Trim();
                    persona.strNombre = this.txtNombre.Text.Trim();
                    persona.strAMaterno = this.txtAMaterno.Text.Trim();
                    persona.strAPaterno = this.txtAPaterno.Text.Trim();
                    persona.idCatSexo = int.Parse(this.ddlSexo.Text);
                    //El calendar
                    //string fecha = txtDia.Text+"/"+txtMes.Text+"/"+txtAnio;
                    //DateTime dt = DateTime.Parse(fecha);

                    //DateTime fechaNacimiento = this.dteCalendar.SelectedDate.Date;
                    //persona.dtFechaDNaci = dt;

                    DateTime dateTime = Convert.ToDateTime(txtMes.Text + "/" + txtDia.Text + "/" + txtAnio.Text, CultureInfo.InvariantCulture);
                    persona.dtFechaDNaci = dateTime;

                    //agregar hermanos
                    persona.intNumHermano =  int.Parse(this.txtNumeroHermanos.Text);
                    //Correo Electronico
                    persona.strCorreoElectronico = this.txtCorreoElectronico.Text.Trim();
                    //Codigo postal
                    persona.intCodigoPostal = int.Parse(this.txtCodigoPostal.Text);
                    //RFC
                    persona.strRFC = this.txtRFC.Text.Trim();
                    dcGuardar.SubmitChanges();
                    this.showMessage("El registro se edito correctamente.");
                    this.Response.Redirect("~/PersonaPrincipal.aspx", false);
                }
            }
            catch (Exception _e)
            {
                this.showMessageException(_e.Message);
                error(_e.ToString());
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            try
            {              
                this.Response.Redirect("~/PersonaPrincipal.aspx", false);
            }
            catch (Exception _e)
            {
                this.showMessage("Ha ocurrido un error inesperado");
                error(_e.ToString());
            }
        }

        protected void ddlSexo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int idSexo = int.Parse(this.ddlSexo.Text);
                Expression<Func<CatSexo, bool>> predicateSexo = c => c.id == idSexo;
                predicateSexo.Compile();
                List<CatSexo> lista = dcGlobal.GetTable<CatSexo>().Where(predicateSexo).ToList();
                CatSexo catTemp = new CatSexo();            
                this.ddlSexo.DataTextField = "strValor";
                this.ddlSexo.DataValueField = "id";
                this.ddlSexo.DataSource = lista;
                this.ddlSexo.DataBind();
            }
            catch (Exception _e)
            {
                this.showMessage("Ha ocurrido un error inesperado");
                error(_e.ToString());
            }
        }

        #endregion
        public bool validacion(UTTT.Ejemplo.Linq.Data.Entity.Persona _persona, ref String _mensaje, string dia, string mes, string anio) {
            if (_persona.idCatSexo.Equals(-1)) {
                _mensaje = "seleccione el sexo";
                return false;
            }
            if (_persona.strClaveUnica.Equals(String.Empty)) {
                _mensaje = "clave es obligatorio";
            }
            if (_persona.strClaveUnica.Length != 3) {
                _mensaje = "clave unica no valida";
                return false;
            }
            if (_persona.strNombre.Length > 50) {
                _mensaje = "Solo se permienten 50 caracteres en Nombre";
                return false;
            }
			if (_persona.strNombre.Equals(String.Empty)) {
                _mensaje = "Nombre esta vacio";
                return false;
            }
            if (_persona.strAPaterno.Equals(String.Empty)) {
                _mensaje = "Apellido paterno esta Vacio";
                return false;
            }
            if (_persona.strAPaterno.Length>50) {
                _mensaje = "Solo se permienten 50 caracteres en Apellido Paterno";
                return false;
            }
            if (_persona.strAMaterno.Length>50) {
                _mensaje = "Solo se permienten 50 caracteres en Apellido Materno";
                return false;
            }
            int Validador = (int.Parse(txtDia.Text)*24*60*60)+(int.Parse(txtMes.Text) * 30 * 24 * 60 * 60)+((2021-int.Parse(txtAnio.Text))*365*24*60*60);
            int mayor = 568024668;
            if (Validador < mayor) {
                _mensaje = "Eres menor de edad";
                return false;
            }
            if (_persona.intNumHermano > 20 ) {
                _mensaje = "Numero de hermanos no valido";
                return false;
            }
            if (_persona.intNumHermano<0) {
                _mensaje = "Numero de hermanos no valido";
                return false;
            }
            if (_persona.intNumHermano.ToString() == "" ) {
                _mensaje = "El campo Numero de hermanos es requerido ";
                return false;
            }
			if (_persona.strCorreoElectronico.Equals(String.Empty) ) {
                _mensaje = "El campo Correo Electronico es requerido ";
                return false;
            }
			//Validar Email
			Regex patternCorreo = new Regex("w+([-+.']w+)*@w+([-.]w+)*.w+([-.]w+)*");
            bool respuesta = patternCorreo.IsMatch(_persona.strCorreoElectronico.ToString()); 
            if (respuesta)
			{
                _mensaje = "Formato no admitido para Email";
                return false;
            }

			if (_persona.strRFC.Equals(String.Empty))
			{
				_mensaje = "El campo RFC es requerido ";
				return false;
			}

			//Validar RFC
			Regex patternRFC = new Regex("^([A-ZÑ\x26]{3,4}([0-9]{2})(0[1-9]|1[0-2])(0[1-9]|1[0-9]|2[0-9]|3[0-1]))((-)?([A-Zd]{3}))?$");
            bool respues = patternRFC.IsMatch(_persona.strRFC.ToString());
            if (respues)
			{
                _mensaje = "Formato no admitido para RFC";
                return false;
            }
			//
			if (_persona.intCodigoPostal.ToString().Equals(String.Empty))
            {
                _mensaje = "El campo RFC es requerido ";
                return false;
            }
            //Regex patternCodigoP = new Regex("^[0-5][1-9]{3}[0-9]$");
            //bool menCP = ;
            //if (patternCodigoP.IsMatch(_persona.intCodigoPostal.ToString()))
            //{
            //    _mensaje = "Formato no admitido para Codigo Postal";
            //    return false;
            //}

            return true;
        }
        public bool validacionHTML( ref String _mensaje) {
            CtrlvalidaInyeccion valida = new CtrlvalidaInyeccion();
            string mensaheFuncion = string.Empty;
            if (valida.htmlInyectionValida(this.txtClaveUnica.Text.Trim(), ref mensaheFuncion, "Clave Unica", ref this.txtClaveUnica))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtNombre.Text.Trim(), ref mensaheFuncion, "Nombre", ref this.txtNombre))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtAPaterno.Text.Trim(), ref mensaheFuncion, "A paterno", ref this.txtAPaterno))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtAMaterno.Text.Trim(), ref mensaheFuncion, "A Materno", ref this.txtAMaterno))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtAMaterno.Text.Trim(), ref mensaheFuncion, "A Materno", ref this.txtAMaterno))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtDia.Text.Trim(), ref mensaheFuncion, "Dia", ref this.txtDia))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtMes.Text.Trim(), ref mensaheFuncion, "Mes", ref this.txtMes))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtAnio.Text.Trim(), ref mensaheFuncion, "Año", ref this.txtAnio))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtNumeroHermanos.Text.Trim(), ref mensaheFuncion, "Numero Hermanos", ref this.txtNumeroHermanos))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtCorreoElectronico.Text.Trim(), ref mensaheFuncion, "Correo Electronico", ref this.txtCorreoElectronico))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtRFC.Text.Trim(), ref mensaheFuncion, "RFC", ref this.txtRFC))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtCodigoPostal.Text.Trim(), ref mensaheFuncion, "Codigo Posgal", ref this.txtCodigoPostal))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            return true;
        }
        public bool validacionSQL( ref String _mensaje) 
        {
            CtrlvalidaInyeccion valida = new CtrlvalidaInyeccion();
            string mensaheFuncion = string.Empty;
            if (valida.SQLInyectionValida(this.txtClaveUnica.Text.Trim(), ref mensaheFuncion, "Clave Unica", ref this.txtClaveUnica))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.SQLInyectionValida(this.txtNombre.Text.Trim(), ref mensaheFuncion, "Nombre", ref this.txtNombre)) 
                {
                _mensaje = mensaheFuncion;
                return false;
                }
            if (valida.SQLInyectionValida(this.txtAPaterno.Text.Trim(), ref mensaheFuncion, "Apellido paterno", ref this.txtAPaterno))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.SQLInyectionValida(this.txtAMaterno.Text.Trim(), ref mensaheFuncion, "Apellido Materno", ref this.txtAMaterno))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.SQLInyectionValida(this.txtAMaterno.Text.Trim(), ref mensaheFuncion, "Apellido Materno", ref this.txtAMaterno))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.SQLInyectionValida(this.txtDia.Text.Trim(), ref mensaheFuncion, "Dia", ref this.txtDia))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.SQLInyectionValida(this.txtMes.Text.Trim(), ref mensaheFuncion, "Mes", ref this.txtMes))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.SQLInyectionValida(this.txtAnio.Text.Trim(), ref mensaheFuncion, "Año", ref this.txtAnio))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.SQLInyectionValida(this.txtNumeroHermanos.Text.Trim(), ref mensaheFuncion, "Hermanos", ref this.txtNumeroHermanos))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.SQLInyectionValida(this.txtCorreoElectronico.Text.Trim(), ref mensaheFuncion, "Correo", ref this.txtCorreoElectronico))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.SQLInyectionValida(this.txtRFC.Text.Trim(), ref mensaheFuncion, "RFC", ref this.txtRFC))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            if (valida.SQLInyectionValida(this.txtCodigoPostal.Text.Trim(), ref mensaheFuncion, "Codigo postal", ref this.txtCodigoPostal))
            {
                _mensaje = mensaheFuncion;
                return false;
            }
            return true;
        }
        public void error(string error)
        {
            string body = error;
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("18301044@uttt.edu.mx", "MJR9416M");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("18301044@uttt.edu.mx", "Error en el servidor Ejemplo Estudiante");
            mail.To.Add(new MailAddress("18301044@uttt.edu.mx"));
            mail.Subject = ("Error");
            mail.Body = body;

            smtp.Send(mail);
        }


        #region Metodos

        public void setItem(ref DropDownList _control, String _value)
        {
            foreach (ListItem item in _control.Items)
            {
                if (item.Value == _value)
                {
                    item.Selected = true;
                    break;
                }
            }
            _control.Items.FindByText(_value).Selected = true;
        }

        #endregion
    }
}