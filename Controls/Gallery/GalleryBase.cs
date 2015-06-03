using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Tvinci.Web.Controls.Gallery;
using Tvinci.Data.DataLoader;

namespace Tvinci.Web.Controls.Gallery
{
    public class StatusUpdatedEventArgs : EventArgs
    {
        public enum eStatus
        {
            NoData,
            Finished,
            Error
        }

        public eStatus Status { get; private set; }

        public StatusUpdatedEventArgs(eStatus status)
        {
            Status = status;
        }
    }

    [PersistChildren(true)]
    [ParseChildren(false)]
    public abstract class GalleryBase : PlaceHolder, INamingContainer, IGallery
    {        
        private bool m_isControlLoaded = false;
        

        public event EventHandler<StatusUpdatedEventArgs> GallerStatusUpdated;

        #region Fields & Properties
        bool m_dataBinded = false;
        Dictionary<object, List<IGalleryPart>> m_parts = new Dictionary<object, List<IGalleryPart>>();
                        
        #endregion

        #region Override methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            m_isControlLoaded = true;
            if (m_dataBinded)
            {                                
                this.DataBind();
            }

            
        }

        protected virtual bool CanUpdateGallery()
        {
            return true;
        }

        public override void DataBind()
        {
			if (m_isControlLoaded)
			{
                if (executeSyncProcess())
                {
                    base.DataBind();
                }
			}
			else
			{
				m_dataBinded = true;
			}
        }
     
        #endregion


        private bool executeSyncProcess()
        {          
            try
            {
                this.Visible = true;
                if (!CanUpdateGallery())
                {
                    this.Visible = false;
                    OnGalleryStatusUpdated(new StatusUpdatedEventArgs(StatusUpdatedEventArgs.eStatus.NoData));
                    return false;
                }

                ReRegisterParts();

                PreSync();

                SyncControl();

                PostSync();

                OnGalleryStatusUpdated(new StatusUpdatedEventArgs(StatusUpdatedEventArgs.eStatus.Finished));  

                return true;
            }
            catch (Exception)
            {
                this.Visible = false;
                OnGalleryStatusUpdated(new StatusUpdatedEventArgs(StatusUpdatedEventArgs.eStatus.Error));
                return false;                                
            }            
        }

        protected virtual void PostSync()
        {            
            return;
        }

        protected virtual void PreSync()
        {
            return;
        }

        protected virtual void HandlePart(IGalleryPart part)
        {
            return;
        }

        protected virtual void SyncControl()
        {
            foreach (KeyValuePair<object, List<IGalleryPart>> item in m_parts)
            {
                foreach (IGalleryPart part in item.Value)
                {                                        
                    HandlePart(part);                                            
                }
            }
        }
        protected void OnGalleryStatusUpdated(StatusUpdatedEventArgs args)
        {
            if (GallerStatusUpdated != null)
            {
                GallerStatusUpdated(this, args);
            }
        }

        #region Public methods

        public void ReRegisterParts()
        {
            m_parts.Clear();

            searchPart(this,registerPart);
        }

        protected virtual bool shouldSearchForParts(Control control)
        {
            return true;
        }


        public delegate void HandlePartDelegate(IGalleryPart part);
        public void HandleInnerControlParts(GalleryPart root, HandlePartDelegate method)
        {
            searchPart(root, method);
        }

        protected void searchPart(Control parent, HandlePartDelegate method)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is IGalleryPart)
                {
                    method((IGalleryPart)child);                    
                }
                else if (child is GalleryBase)
                {
                    continue;
                }

                //if (child.Visible)
                //{                                        
                    if (shouldSearchForParts(child))
                    {
                        searchPart(child, method);
                    }
                //}
            }
        }


        private void registerPart(IGalleryPart part)
        {
            if (!IsValidGalleryPart(part))
            {
                throw new Exception("");
            }

            List<IGalleryPart> list;

            if (!m_parts.TryGetValue(part.HandlerID, out list))
            {
                list = new List<IGalleryPart>();
                m_parts.Add(part.HandlerID, list);                
                part.HandleAddedToGallery(this);
            }

            list.Add(part);
        }

        protected virtual bool IsValidGalleryPart(IGalleryPart instance)
        {
            return true;
        }
        #endregion

        #region IGallery Members
        
        public event EventHandler<CommandEventArgs> CommandChangedInProxy;

        void IGallery.RaiseCommandChangedInProxy(CommandEventArgs e)
        {
            OnCommandChangedInProxy(e);
        }

        void OnCommandChangedInProxy(CommandEventArgs e)
        {
            if (CommandChangedInProxy != null)
            {
                CommandChangedInProxy(this, e);
            }
        }
        #endregion
    }
}


