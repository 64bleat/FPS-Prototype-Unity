/*
    Some packages are creating errors when building and
    need to be altered to try to fix the problem instead
    of crying about it. They are immutable and keep resetting
    so this file keeps the fixes so I can paste them back 
    in when they are needed 

    ProBuilderMeshFunction.cs
    void RefreshColors()
    {
        Mesh m = filter.sharedMesh;

        if (m_Colors.Length == m.colors.Length)
            m.colors = m_Colors;
        else
        {
            Debug.LogWarning($"FIXING COLOR ARRAY ON {filter.gameObject.name}!");
            Color[] fix = new Color[m.colors.Length];
            int length = Mathf.Min(fix.Length, m_Colors.Length);

            for (int i = 0; i < length; i++)
                fix[i] = m_Colors[i];

            m.colors = fix;
            filter.sharedMesh.colors = fix;
        }
    }
 

 * */
