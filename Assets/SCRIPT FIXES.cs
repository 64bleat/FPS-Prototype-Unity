/*
    Some packages are creating errors when building and
    need to be altered to try to fix the problem instead
    of crying about it. They are immutable and keep resetting
    so this file keeps the fixes so I can paste them back 
    in when they are needed 

    --- Replace in ProBuilderMeshFunction.cs ---

        void RefreshColors()
        {
            MeshFilter filter = this.filter;
            Mesh mesh = filter.sharedMesh;

            if (mesh.colors.Length == m_Colors.Length)
                mesh.colors = m_Colors;
            else
            {
                Debug.LogWarning($"FIXING COLOR ARRAY ON {gameObject.name}!", gameObject);

                Color[] fix = new Color[mesh.colors.Length];

                for (int i = 0; i < fix.Length; i++)
                    if (i < m_Colors.Length)
                        fix[i] = m_Colors[i];
                    else
                        fix[i] = Color.white;

                mesh.colors = fix;
                m_Colors = fix;
                filter.sharedMesh = mesh;
            }
        }
 

 * */
