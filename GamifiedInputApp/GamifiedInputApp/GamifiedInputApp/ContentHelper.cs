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
        ExpPointerInputObserver m_pointerInputObvserver;
        
        SpriteVisual m_backgroundVisual;
        CompositionColorBrush m_redBrush;
        CompositionColorBrush m_blueBrush;
        bool m_red = true;

        public ContentHelper(Compositor compositor)
        {
            m_compositor = compositor;
            m_content = ExpCompositionContent.Create(compositor);

            m_backgroundVisual = m_compositor.CreateSpriteVisual();
            m_redBrush = m_compositor.CreateColorBrush(Microsoft.UI.Colors.Red);
            m_blueBrush = m_compositor.CreateColorBrush(Microsoft.UI.Colors.Blue);
            m_backgroundVisual.Brush = m_redBrush;
            m_backgroundVisual.Size = new System.Numerics.Vector2(400, 400);
            m_content.Root = m_backgroundVisual;

            m_inputSite = ExpInputSite.GetOrCreateForContent(m_content);
            m_pointerInputObvserver = ExpPointerInputObserver.CreateForInputSite(m_inputSite);
            m_pointerInputObvserver.PointerPressed += M_pointerInputObvserver_PointerPressed;
        }

        public ExpCompositionContent Content
        {
            get { return m_content; }
        }

        public ExpInputSite InputSite
        {
            get { return m_inputSite; }
        }

        private void M_pointerInputObvserver_PointerPressed(
            ExpPointerInputObserver sender, 
            ExpPointerEventArgs args)
        {
            m_red = !m_red;

            if (m_red)
            {
                m_backgroundVisual.Brush = m_redBrush;
            }
            else
            {
                m_backgroundVisual.Brush = m_blueBrush;
            }
        }
    }
}
