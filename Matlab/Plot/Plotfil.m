clc; clear;

content = fileread( 'Case4_topOpt_disp.txt' ) ;
data = textscan( content, '%s','HeaderLines',2) ;
text = data{1};

k = 1;
c = 1;
r = 1;

for i = 1: length(text)
    
    if strcmp(text(i), "plots")
        temp = cell2mat(text(i+1));
        it = str2double(temp);
    end
    
    if strcmp(text(i), "numpoints")
        temp = cell2mat(text(i+1));
        number = str2double(temp);
    end
    
    if strcmp(text(i), "logarithmic")
        log = cell2mat(text(i+1));
    end
    
    if strcmp(text(i), "title")
        Title = cell2mat(text(i+1));
        Title= strrep(Title,'_',' ');
    end
    if strcmp(text(i), "xlabel")
        xLabel = cell2mat(text(i+1));
        xLabel = strrep(xLabel,'_',' ');
    end
    if strcmp(text(i), "ylabel")
        yLabel = cell2mat(text(i+1));
        yLabel = strrep(yLabel,'_',' ');
    end

    if strcmp(text(i), "xaxis")
        for j = i:(i+number-1)
             cell = cell2mat(text(j+1));
             xaxis((j+1)-i) = str2double(cell);
        end
    end
    
    if strcmp(text(i), "numbx")
        temp = cell2mat(text(i+1));
        numbx = str2double(temp);
    end
    
    if strcmp(text(i), "xaxisPlot")
        for j = i:(i+numbx-1)
             cell = cell2mat(text(j+1));
             xaxisPlot((j+1)-i) = str2double(cell);
        end
    end
    
    if strcmp(text(i), "yaxis")
       for l = i:(i+number-1);
             cell = cell2mat(text(l+1));
             yaxis(((l+1)-i),k) = str2double(cell);
        end
        k = k+1;
    end
    if strcmp(text(i), "color")
        colors(c) = text(i+1);
        c =c +1;
    end
    
    if strcmp(text(i), "legend")
        Legend(r) = text(i+1);
        Legend(r) = strrep(Legend(r),'_',' ');
        r = r +1;
    end
    
end


xq2 = 1:0.1:xaxis(length(xaxis))



%figure(1)
figure('Renderer', 'painters', 'Position', [10 10 900 600])
hold on

set(gca, 'FontName', 'Times New Roman')
set(gca,'fontsize',10)
%legend([plot1,plot2],'t_{i}','t_{ri}','Location','southeast');
%axis tight
%grid minor


if(strcmp(log,"true"))
    set(gca, 'XScale', 'log')
end

if it == 1
    vq1 = interp1(xaxis,yaxis(:,1),xq2,'pchip');
    p1 = plot(xaxis,yaxis(:,1),'o',xq2,vq1,cell2mat(colors(1)),'LineWidth',1,'MarkerSize',4);
end

if it == 2
    vq1 = interp1(xaxis,yaxis(:,1),xq2)
    vq2 = interp1(xaxis,yaxis(:,2),xq2)
    p1 = plot(xaxis,yaxis(:,1),'o',xq2,vq1,cell2mat(colors(1)),'LineWidth',1,'MarkerSize',4);
    p2 = plot(xaxis,yaxis(:,2),'o',xq2,vq2,cell2mat(colors(2)),'LineWidth',1,'MarkerSize',4);
end

if it == 3
    vq1 = interp1(xaxis,yaxis(:,1),xq2,'pchip');
    vq2 = interp1(xaxis,yaxis(:,2),xq2,'pchip');
    vq3 = interp1(xaxis,yaxis(:,3),xq2,'pchip');
    p1 = plot(xaxis,yaxis(:,1),'o',xq2,vq1,cell2mat(colors(1)),'LineWidth',1,'MarkerSize',4);
    p2 = plot(xaxis,yaxis(:,2),'*',xq2,vq2,cell2mat(colors(2)),'LineWidth',1,'MarkerSize',4);
    p3 = plot(xaxis,yaxis(:,3),'x',xq2,vq3,cell2mat(colors(3)),'LineWidth',1,'MarkerSize',4);
    
    %legend([p1,p2],'t','t','Location','southeast');
    
    h = [p1(1);p2(1);p3(1)];
 % Now call the legend function passing the handle h and specify the text
    legend(h,cell2mat(Legend(1)),cell2mat(Legend(2)),cell2mat(Legend(3)));
    
end

disp(xaxisPlot)
ax = gca;
ax.XMinorTick = 'on';
ax.XAxis.TickValues = xaxisPlot;

xlabel(xLabel)
ylabel(yLabel)
title(Title)