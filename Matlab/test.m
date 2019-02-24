a = '{lx*ly*lz*((0.22*d11)/lx^2 + (0.22*d44)/ly^2 + (0.22*d55)/lz^2)';

s = a;

for i = 1: length(a)
   if (strcmp(a(i),'^'))
       if(strcmp(a(i-1),'x'))
          s = strrep(s,'lx^2','(lx*lx)');
       elseif(strcmp(a(i-1),'y'))
          s = strrep(s,'ly^2','(ly*ly)');
       elseif(strcmp(a(i-1),'z'))
          s = strrep(s,'lz^2','(lz*lz)');
       end
   end
end

disp(s)