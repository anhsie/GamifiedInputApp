using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental;

namespace GamifiedInputApp
{
    public class ContentHelper
    {
        Compositor m_compositor;
        ExpCompositionContent m_content;
        ExpInputSite m_inputSite;
        SpriteVisual m_backgroundVisual;

        public ContentHelper(Compositor compositor)
        {
            m_compositor = compositor;
            m_content = ExpCompositionContent.Create(compositor);
            m_content.AppData = this;

            m_backgroundVisual = m_compositor.CreateSpriteVisual();
            m_backgroundVisual.Brush = m_compositor.CreateColorBrush(Microsoft.UI.Colors.White);
            m_backgroundVisual.Size = new System.Numerics.Vector2(400, 400);
            m_content.Root = m_backgroundVisual;

            m_inputSite = ExpInputSite.GetOrCreateForContent(m_content);

            m_content.StateChanged += M_content_StateChanged;
        }

        private void M_content_StateChanged(ExpCompositionContent sender, ExpCompositionContentEventArgs args)
        {
            m_backgroundVisual.Size = m_content.ActualSize;
        }

        public ExpCompositionContent Content
        {
            get { return m_content; }
        }

        public ExpInputSite InputSite
        {
            get { return m_inputSite; }
        }

        public SpriteVisual RootVisual
        {
            get { return m_backgroundVisual; }
        }
    }
}
